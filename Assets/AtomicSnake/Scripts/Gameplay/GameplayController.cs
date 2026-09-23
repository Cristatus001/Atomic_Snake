using System.Collections;
using System.Collections.Generic;
using AtomicSnake.Chemistry;
using AtomicSnake.Field;
using AtomicSnake.Runtime;
using AtomicSnake.Snake;
using AtomicSnake.UI;
using UnityEngine;

namespace AtomicSnake.Gameplay
{
    public class GameplayController : MonoBehaviour
    {
        const int StartLength = 4;
        const float StartHeadingDegrees = 0f;
        const float PickupDistance = 0.5f;
        const float TileMinSeparation = 1f;
        const float ObstacleCollisionDistance = 0.45f;
        const float ClockFreezeSeconds = 10f;
        const float MapSpawnMin = 12f, MapSpawnMax = 20f;
        const float ClockSpawnMin = 15f, ClockSpawnMax = 25f;

        SnakeController _snake;
        UIManager _ui;
        Transform _fieldRoot;
        WordTileView _wordTilePrefab;
        PowerUpView _mapPrefab;
        PowerUpView _clockPrefab;
        ObstacleView _obstaclePrefab;

        LevelDefinition _level;
        int _levelIndex;
        int _questionIndex;
        int _atomicNumber;
        List<string> _sequence;
        int _sequencePointer;
        readonly List<WordTileView> _activeTiles = new();
        readonly List<ObstacleView> _obstacles = new();

        int _score;
        float _timeRemaining;
        float _freezeRemaining;
        float _driftTimer;
        float _mapSpawnTimer;
        float _clockSpawnTimer;
        PowerUpView _activeMap;
        PowerUpView _activeClock;
        bool _playing;
        bool _busy; // true while a transient message is showing and gameplay is paused

        public void Initialize(SnakeController snake, Transform fieldRoot, UIManager ui, WordTileView wordTilePrefab,
            PowerUpView mapPrefab, PowerUpView clockPrefab, ObstacleView obstaclePrefab)
        {
            _snake = snake;
            _fieldRoot = fieldRoot;
            _ui = ui;
            _wordTilePrefab = wordTilePrefab;
            _mapPrefab = mapPrefab;
            _clockPrefab = clockPrefab;
            _obstaclePrefab = obstaclePrefab;
            _snake.OnSelfCollision += () => HandleCrash("You bit your own tail!");
            _snake.OnHeadMoved += HandleHeadMoved;
        }

        public void StartLevel(int levelIndex)
        {
            _levelIndex = levelIndex;
            _level = LevelCatalog.All[levelIndex];
            _score = 0;
            _questionIndex = 0;
            _timeRemaining = _level.TimeLimitSeconds;
            _freezeRemaining = 0f;
            _ui.SetTimerFrozen(false);
            _playing = true;
            _busy = false;

            _snake.ResetSnake(Vector2.zero, StartLength, StartHeadingDegrees);
            _snake.SetActive(true);
            _snake.SetSpeed(_level.SnakeSpeed);

            SpawnObstacles();
            ResetPowerUpTimers();

            _ui.ShowHud();
            StartQuestion();
        }

        void StartQuestion()
        {
            _atomicNumber = _level.AtomicNumbers[_questionIndex];
            _sequence = IupacNaming.GetCollectSequence(_atomicNumber);
            _sequencePointer = 0;
            _driftTimer = _level.DriftIntervalSeconds;

            _ui.UpdateHud(_level.DisplayName, _questionIndex + 1, _level.ElementCount, _atomicNumber, _score, _timeRemaining);
            _ui.UpdateCollectedTray(_sequence, _sequencePointer);

            SpawnTilesForSequence();
        }

        void SpawnTilesForSequence()
        {
            ClearTiles();

            var occupied = CurrentOccupiedPositions();
            foreach (string part in _sequence)
            {
                var pos = FindSpawnPosition(occupied);
                occupied.Add(pos);
                _activeTiles.Add(WordTileView.Create(_wordTilePrefab, _fieldRoot, part, pos));
            }

            for (int i = 0; i < _level.DecoyTileCount; i++)
            {
                var pos = FindSpawnPosition(occupied);
                occupied.Add(pos);
                string decoyText = IupacNaming.RandomUnrelatedRoot(_atomicNumber);
                _activeTiles.Add(WordTileView.Create(_wordTilePrefab, _fieldRoot, decoyText, pos));
            }
        }

        void ClearTiles()
        {
            foreach (var tile in _activeTiles)
                if (tile != null) Destroy(tile.gameObject);
            _activeTiles.Clear();
        }

        List<Vector2> CurrentOccupiedPositions()
        {
            var occupied = new List<Vector2>(_snake.BodySamples);
            foreach (var obstacle in _obstacles) occupied.Add(obstacle.Position);
            return occupied;
        }

        Vector2 FindSpawnPosition(List<Vector2> occupied)
        {
            for (int attempt = 0; attempt < 200; attempt++)
            {
                var candidate = GridUtil.RandomPointInField();
                bool clear = true;
                foreach (var p in occupied)
                {
                    if (Vector2.Distance(p, candidate) < TileMinSeparation)
                    {
                        clear = false;
                        break;
                    }
                }
                if (clear) return candidate;
            }
            return GridUtil.RandomPointInField();
        }

        void SpawnObstacles()
        {
            foreach (var obstacle in _obstacles)
                if (obstacle != null) Destroy(obstacle.gameObject);
            _obstacles.Clear();

            var occupied = new List<Vector2>(_snake.BodySamples);
            for (int i = 0; i < _level.ObstacleCount; i++)
            {
                var pos = FindSpawnPosition(occupied);
                occupied.Add(pos);
                _obstacles.Add(ObstacleView.Create(_obstaclePrefab, _fieldRoot, pos));
            }
        }

        void ResetPowerUpTimers()
        {
            if (_activeMap != null) Destroy(_activeMap.gameObject);
            if (_activeClock != null) Destroy(_activeClock.gameObject);
            _activeMap = null;
            _activeClock = null;
            _mapSpawnTimer = Random.Range(MapSpawnMin, MapSpawnMax);
            _clockSpawnTimer = Random.Range(ClockSpawnMin, ClockSpawnMax);
        }

        void Update()
        {
            if (!_playing || _busy) return;

            UpdateTimer();
            UpdateDrift();
            UpdatePowerUpSpawns();
        }

        void UpdateTimer()
        {
            if (_freezeRemaining > 0f)
            {
                _freezeRemaining -= Time.deltaTime;
                if (_freezeRemaining <= 0f)
                {
                    _freezeRemaining = 0f;
                    _ui.SetTimerFrozen(false);
                }
            }
            else
            {
                _timeRemaining -= Time.deltaTime;
            }

            _ui.UpdateTimer(Mathf.Max(0f, _timeRemaining));

            if (_timeRemaining <= 0f)
                StartCoroutine(HardResetRoutine("Time's up!"));
        }

        void UpdateDrift()
        {
            if (!_level.TilesDrift) return;

            _driftTimer -= Time.deltaTime;
            if (_driftTimer > 0f) return;
            _driftTimer = _level.DriftIntervalSeconds;

            var occupied = CurrentOccupiedPositions();
            foreach (var tile in _activeTiles)
            {
                var pos = FindSpawnPosition(occupied);
                occupied.Add(pos);
                tile.Reposition(pos);
            }
        }

        void UpdatePowerUpSpawns()
        {
            if (_activeMap == null)
            {
                _mapSpawnTimer -= Time.deltaTime;
                if (_mapSpawnTimer <= 0f)
                {
                    var pos = FindSpawnPosition(CurrentOccupiedPositions());
                    _activeMap = PowerUpView.Create(_mapPrefab, _fieldRoot, pos);
                }
            }

            if (_activeClock == null)
            {
                _clockSpawnTimer -= Time.deltaTime;
                if (_clockSpawnTimer <= 0f)
                {
                    var pos = FindSpawnPosition(CurrentOccupiedPositions());
                    _activeClock = PowerUpView.Create(_clockPrefab, _fieldRoot, pos);
                }
            }
        }

        void HandleHeadMoved(Vector2 headPosition)
        {
            if (!_playing || _busy) return;

            if (CheckObstacleCollision(headPosition))
            {
                HandleCrash("You hit an obstacle!");
                return;
            }

            if (_activeMap != null && Vector2.Distance(_activeMap.Position, headPosition) <= PickupDistance)
            {
                Destroy(_activeMap.gameObject);
                _activeMap = null;
                _mapSpawnTimer = Random.Range(MapSpawnMin, MapSpawnMax);
                ShowMapTable();
                return;
            }

            if (_activeClock != null && Vector2.Distance(_activeClock.Position, headPosition) <= PickupDistance)
            {
                Destroy(_activeClock.gameObject);
                _activeClock = null;
                _clockSpawnTimer = Random.Range(ClockSpawnMin, ClockSpawnMax);
                _freezeRemaining += ClockFreezeSeconds;
                _ui.SetTimerFrozen(true);
            }

            WordTileView hit = null;
            foreach (var tile in _activeTiles)
            {
                if (Vector2.Distance(tile.Position, headPosition) > PickupDistance) continue;
                hit = tile;
                break;
            }
            if (hit == null) return;

            _activeTiles.Remove(hit);
            Destroy(hit.gameObject);

            bool correct = string.Equals(hit.Text, _sequence[_sequencePointer], System.StringComparison.OrdinalIgnoreCase);
            if (correct)
            {
                _sequencePointer++;
                _score += 10;
                _snake.QueueGrowth(1);
                _ui.UpdateCollectedTray(_sequence, _sequencePointer);
                _ui.UpdateScore(_score);

                if (_sequencePointer >= _sequence.Count)
                    HandleQuestionComplete();
            }
            else
            {
                _sequencePointer = 0;
                _ui.UpdateCollectedTray(_sequence, _sequencePointer);
                SpawnTilesForSequence();
            }
        }

        bool CheckObstacleCollision(Vector2 headPosition)
        {
            foreach (var obstacle in _obstacles)
            {
                if (Vector2.Distance(obstacle.Position, headPosition) < ObstacleCollisionDistance)
                    return true;
            }
            return false;
        }

        // The map pickup shows the digit-root reference table, per the
        // reference game — paused until the player closes it themselves,
        // not a timed reveal.
        void ShowMapTable()
        {
            _busy = true;
            _snake.SetActive(false);
            _ui.ShowMapTable(ResumeAfterMapTable);
        }

        void ResumeAfterMapTable()
        {
            _busy = false;
            _snake.SetActive(true);
        }

        void HandleQuestionComplete()
        {
            _score += 50;
            _ui.UpdateScore(_score);
            _questionIndex++;

            if (_questionIndex >= _level.ElementCount)
            {
                _playing = false;
                _snake.SetActive(false);
                GameProgress.MarkLevelCleared(_levelIndex);
                ClearTiles();
                _ui.ShowLevelComplete(_level.DisplayName, _score);
                return;
            }

            StartQuestion();
        }

        void HandleCrash(string message)
        {
            if (!_playing || _busy) return;
            StartCoroutine(HardResetRoutine(message));
        }

        IEnumerator HardResetRoutine(string message)
        {
            _busy = true;
            _snake.SetActive(false);
            _ui.ShowMessage(message);
            yield return new WaitForSeconds(1.2f);
            _ui.HideMessage();

            _score = 0;
            _questionIndex = 0;
            _timeRemaining = _level.TimeLimitSeconds;
            _freezeRemaining = 0f;
            _ui.SetTimerFrozen(false);
            _snake.ResetSnake(Vector2.zero, StartLength, StartHeadingDegrees);
            _snake.SetActive(true);
            _snake.SetSpeed(_level.SnakeSpeed);

            SpawnObstacles();
            ResetPowerUpTimers();

            _busy = false;
            StartQuestion();
        }

        public void StopLevel()
        {
            _playing = false;
            _busy = false;
            _snake.SetActive(false);
            ClearTiles();
            ResetPowerUpTimers();
            foreach (var obstacle in _obstacles)
                if (obstacle != null) Destroy(obstacle.gameObject);
            _obstacles.Clear();
            StopAllCoroutines();
        }
    }
}
