
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
        * - GetNextIndex - получить следующий индекс
        * - HasNext - 
        */
        private List<RouteStep> _steps = new List<RouteStep>();
        private int _currentStepIndex = 0;  // Текущий шаг в маршруте
        private int _lastReturnedIndex = -1; // Последний возвращенный индекс
        private int _startStepIndex = 0;

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
        
        public bool HasNext()
        {
            // Проверяем, остались ли еще шаги в маршруте
            return _currentStepIndex < _steps.Count;
        }
        
        public void Reset()
        {
            _currentStepIndex = _startStepIndex;
        }

        public void SetStartStepIndex(int index)
        {
            _startStepIndex = index;
        }
        
        public List<int> BuildRoute()
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