
namespace MinesServer.GameShit.Programmator
{
    public class RouteBuilder
    {
        /*
        * Построение пути программатора
        * - задает порядок выполнения с поддержкой ветвления по условию да/нет
        * Применение: обойти по порядку всю программу, добавив в путь индексы последовательности
        * основные методы: 
        * - AddStep - добавить следующий индекс
        * - AddConditionalStep - добавить ветвление с условием
        * - AddStartStep - добавить следующий индекс, который станет точкой старта
        * - AddRecursiveStep - добавить goto индекс, закинуть в стек следующий индекс
        * - AddReturnStep - добавить индекс из стека
        * ________________________________________________________________________________________
        * - GetNextIndex - получить следующий индекс, реализуя все функции (точка старта, ветвление, рекурсия и тд)
        * - 
        */
        private List<RouteStep> _steps = new List<RouteStep>();
        private int _currentStepIndex = 0;  // Текущий шаг в маршруте
        private int _lastReturnedIndex = -1; // Последний возвращенный индекс
        private int _startStepIndex = 0;

        private int _deathStepIndex = -1;
        private Stack<int> _returnPoints = new Stack<int>(); // Стек точек возврата из индексов шагов

        public void AddStep(int nextIndex)
        {
            _steps.Add(new RouteStep { NextIndex = nextIndex });
        }
        
        public void AddConditionalStep(Func<bool> condition, int trueIndex, int falseIndex)
        {
            _steps.Add(new ConditionalRouteStep
            {
                Condition = condition,
                TrueIndex = trueIndex,
                FalseIndex = falseIndex
            });
        }
        public void AddStartStep(int nextIndex, Action externalMethod = null)
        {
            _steps.Add(new RouteStepWithMethod
            {
                NextIndex = nextIndex,
                ExternalMethod = externalMethod ?? (() => SetStartStepIndex(_steps.Count))
            });
        }
        public void AddRecursiveStep(int nextIndex, int gotoIndex, Action externalMethod = null)
        {
            _steps.Add(new RouteStepWithMethod
            {
                NextIndex = gotoIndex,
                ExternalMethod = externalMethod ?? (() => SetReturnStepIndex(nextIndex))
            });
        }
        public void AddReturnStep()
        {
            if (_returnPoints.Count > 0)
            {
                var returnStepIndex = _returnPoints.Pop();
                AddStep(returnStepIndex);
            }
            else
            {
                AddEndStep(); // заглушка, поменять
                // ERROR: выполнение возврата функции вне функции
                // TODO: реализовать ошибку с завершением работы программатора
            }
        }
        public void AddEndStep()
        {
            Reset();
            _steps.Add(new RouteStep { NextIndex = _startStepIndex });
        }
        public void AddDeathStep(int nextIndex, int gotoIndex, Action externalMethod = null)
        {
            _steps.Add(new RouteStepWithMethod
            {
                NextIndex = nextIndex,
                ExternalMethod = externalMethod ?? (() => SetDeathStepIndex(gotoIndex))
            });
        }
        public void FixStep(int it, int nextIndex, int type = 0)
        {   // исправляет существующие шаги
            var index = 0;
            if (it < _steps.Count())
            {
                switch (type)
                {
                    case 0:
                        _steps[it] = new RouteStep { NextIndex = nextIndex };
                        break;
                    case 1: // Command.CALL_FUNC
                        index = _steps[it].NextIndex;
                        _steps[it] = new RouteStepWithMethod
                            {
                                NextIndex = index,
                                ExternalMethod = (() => SetReturnStepIndex(nextIndex))
                            };
                        break;
                    case 9: // Command.RESPAWN_TO
                        index = _steps[it].NextIndex;
                        _steps[it] = new RouteStepWithMethod
                            {
                                NextIndex = index,
                                ExternalMethod = (() => SetDeathStepIndex(nextIndex))
                            };
                        break;
                }
            }
        }
        
        public int GetNextIndex()
        {
            if (!HasNext())
                Reset(); // бесконечный цикл
                
            var step = _steps[_currentStepIndex];
            int nextIndex = step.GetNextIndex();
            _lastReturnedIndex = nextIndex;
            _currentStepIndex++;
            
            return nextIndex;
        }
        
        public void Reset()
        {
            _currentStepIndex = _startStepIndex;
        }

        public int getLastIndex()
        {
            return _lastReturnedIndex;
        }
        public int getDeathIndex()
        {
            return _deathStepIndex;
        }
        
        private bool HasNext()
        {
            // Проверяем, остались ли еще шаги в маршруте
            return _currentStepIndex < _steps.Count;
        }

        private void SetStartStepIndex(int index)
        {
            _startStepIndex = index;
        }
        private void SetDeathStepIndex(int index)
        {
            _deathStepIndex = index;
        }

        private void SetReturnStepIndex(int index)
        {
            _returnPoints.Push(index);
        }
        
        
        private List<int> BuildRoute()
        {
            var route = new List<int>();
            Reset();
            
            while (HasNext())
            {
                int nextIndex = GetNextIndex();
                route.Add(nextIndex);
            }
            
            return route;
        }
        
        private class RouteStep
        {
            public virtual int NextIndex { get; set; }
            
            public virtual int GetNextIndex()
            {
                return NextIndex;
            }
        }
        
        private class ConditionalRouteStep : RouteStep
        {
            public Func<bool> Condition { get; set; }
            public int TrueIndex { get; set; }
            public int FalseIndex { get; set; }
            
            public override int GetNextIndex()
            {
                return Condition() ? TrueIndex : FalseIndex;
            }
        }

        private class RouteStepWithMethod : RouteStep
        {
            public Action ExternalMethod { get; set; }
            
            public override int GetNextIndex()
            {
                ExternalMethod?.Invoke();
                return NextIndex;
            }
        }
    }
}