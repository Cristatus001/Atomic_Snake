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

        SnakeController _snake;
        UIManager _ui;
        Transform _fieldRoot;

        LevelDefinition _level;
        int _levelIndex;
        int _questionIndex;
        List<string> _sequence;
        int _sequencePointer;
        readonly List<WordTileView> _activeTiles = new();

        int _score;
        float _timeRemaining;
        bool _playing;
        bool _busy; // true while a transient message is showing and gameplay is paused

        public void Initialize(SnakeController snake, Transform fieldRoot, UIManager ui)
        {
            _snake = snake;
            _fieldRoot = fieldRoot;
            _ui = ui;
            _snake.OnSelfCollision += () => HandleCrash("You bit your own tail!");
            _snake.OnHitWall += () => HandleCrash("You crashed into the hedge!");
            _snake.OnHeadMoved += HandleHeadMoved;
        }

        public void StartLevel(int levelIndex)
        {
            _levelIndex = levelIndex;
            _level = LevelCatalog.All[levelIndex];
            _score = 0;
            _questionIndex = 0;
            _timeRemaining = _level.TimeLimitSeconds;
            _playing = true;
            _busy = false;

            _snake.ResetSnake(Vector3.zero, StartLength, StartHeadingDegrees);
            _snake.SetActive(true);

            _ui.ShowHud();
            StartQuestion();
        }

        void StartQuestion()
        {
            int atomicNumber = _level.AtomicNumbers[_questionIndex];
            _sequence = IupacNaming.GetCollectSequence(atomicNumber);
            _sequencePointer = 0;

            _ui.UpdateHud(_level.DisplayName, _questionIndex + 1, _level.ElementCount, atomicNumber, _score, _timeRemaining);
            _ui.UpdateCollectedTray(_sequence, _sequencePointer);

            SpawnTilesForSequence();
        }

        void SpawnTilesForSequence()
        {
            ClearTiles();

            var occupied = new List<Vector3> { _snake.HeadPosition };
            occupied.AddRange(_snake.SegmentPositions);

            foreach (string part in _sequence)
            {
                var pos = FindSpawnPosition(occupied);
                occupied.Add(pos);
                _activeTiles.Add(WordTileView.Create(_fieldRoot, part, pos));
            }
        }

        void ClearTiles()
        {
            foreach (var tile in _activeTiles)
                if (tile != null) Destroy(tile.gameObject);
            _activeTiles.Clear();
        }

        Vector3 FindSpawnPosition(List<Vector3> occupied)
        {
            for (int attempt = 0; attempt < 200; attempt++)
            {
                var candidate = FieldConfig.RandomPointInField();
                bool clear = true;
                foreach (var p in occupied)
                {
                    if (Vector3.Distance(p, candidate) < FieldConfig.TileMinSeparation)
                    {
                        clear = false;
                        break;
                    }
                }
                if (clear) return candidate;
            }
            return FieldConfig.RandomPointInField();
        }

        void Update()
        {
            if (!_playing || _busy) return;

            _timeRemaining -= Time.deltaTime;
            _ui.UpdateTimer(Mathf.Max(0f, _timeRemaining));

            if (_timeRemaining <= 0f)
                StartCoroutine(HardResetRoutine("Time's up!"));
        }

        void HandleHeadMoved(Vector3 headPosition)
        {
            if (!_playing || _busy) return;

            WordTileView hit = null;
            foreach (var tile in _activeTiles)
            {
                if (Vector3.Distance(tile.Position, headPosition) > FieldConfig.PickupDistance) continue;
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
            _snake.ResetSnake(Vector3.zero, StartLength, StartHeadingDegrees);
            _snake.SetActive(true);
            _busy = false;
            StartQuestion();
        }

        public void StopLevel()
        {
            _playing = false;
            _busy = false;
            _snake.SetActive(false);
            ClearTiles();
            StopAllCoroutines();
        }
    }
}
