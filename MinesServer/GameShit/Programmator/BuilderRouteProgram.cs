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
        private Dictionary<int, RouteStep> dict_step = new();
        private int _currentStepIndex = 0;  // Текущий шаг в маршруте
        private int _lastReturnedIndex = -1; // Последний возвращенный индекс
        private int _startStepIndex = 0;

        private int _deathStepIndex = -1;
        private Stack<int> _returnPoints = new Stack<int>(); // Стек точек возврата из индексов шагов
        
        // Логирование
        private bool _enableLogging = true;
        private string _programmerName = "Programmator";

        public RouteBuilder()
        {
        }
        
        public RouteBuilder(string programmerName, bool enableLogging = true)
        {
            _programmerName = programmerName;
            _enableLogging = enableLogging;
        }

        public void AddStep(int index, int nextIndex)
        {
            dict_step.Add(index, new RouteStep { NextIndex = nextIndex });
            Log($"Добавлен обычный шаг: переход к индексу {nextIndex}");
        }
        
        public void AddConditionalStep(int index, Func<bool> condition, int trueIndex, int falseIndex)
        {
            dict_step.Add(index, new ConditionalRouteStep
            {
                Condition = condition,
                TrueIndex = trueIndex,
                FalseIndex = falseIndex
            });
            Log($"Добавлен условный шаг: если условие истинно -> {trueIndex}, иначе -> {falseIndex}");
        }
        
        public void AddStartStep(int index, int nextIndex, Action externalMethod = null)
        {
            dict_step.Add(index, new RouteStepWithMethod
            {
                NextIndex = nextIndex,
                ExternalMethod = externalMethod ?? (() => SetStartStepIndex(nextIndex))
            });
            Log($"⭐ Добавлен стартовый шаг: переход к индексу {_currentStepIndex} (точка входа в программу)");
        }
        
        public void AddRecursiveStep(int index, int nextIndex, int gotoIndex, Action externalMethod = null)
        {
            dict_step.Add(index, new RouteStepWithMethod
            {
                NextIndex = gotoIndex,
                ExternalMethod = externalMethod ?? (() => SetReturnStepIndex(nextIndex))
            });
            Log($"🔄 Добавлен рекурсивный шаг: переход к {gotoIndex}, после возврата -> {nextIndex}");
        }
        
        public void AddReturnStep(int index)
        {
            dict_step.Add(index, new RouteStepUpdate { ExternalMethod = (() => _returnPoints.Pop()) });
            Log($"↩️ Добавлен шаг возврата: возвращаемся к индексу {getReturnIndex()} (стек возврата: {_returnPoints.Count} элементов)");
            
        }
        
        public void AddEndStep(int index)
        {
            dict_step.Add(index, new RouteStepUpdate { ExternalMethod = (() => _startStepIndex) });
            Log($"🏁 Добавлен конечный шаг: возврат к стартовому индексу {_startStepIndex}");
        }
        
        public void AddDeathStep(int index, int nextIndex, int gotoIndex, Action externalMethod = null)
        {
            dict_step.Add(index, new RouteStepWithMethod
            {
                NextIndex = nextIndex,
                ExternalMethod = externalMethod ?? (() => SetDeathStepIndex(gotoIndex))
            });
            Log($"💀 Добавлен шаг смерти: при достижении индекса {nextIndex} активируется точка возрождения {gotoIndex}");
        }
        
        public void FixStep(int it, int nextIndex, int type = 0)
        {   // исправляет существующие шаги
            int index = 0;
            if (it < dict_step.Count())
            {
                switch (type)
                {
                    case 0:
                        dict_step[it] = new RouteStep { NextIndex = nextIndex };
                        Log($"🔧 Исправлен шаг {it}: теперь переход к {nextIndex} (обычный)");
                        break;
                    case 1: // Command.CALL_FUNC
                        index = dict_step[it].NextIndex;
                        dict_step[it] = new RouteStepWithMethod
                            {
                                NextIndex = nextIndex,
                                ExternalMethod = (() => SetReturnStepIndex(index))
                            };
                        Log($"🔧 Исправлен шаг {it}: теперь вызов функции с возвратом к {nextIndex}");
                        break;
                    case 9: // Command.RESPAWN_TO
                        index = dict_step[it].NextIndex;
                        dict_step[it] = new RouteStepWithMethod
                            {
                                NextIndex = nextIndex,
                                ExternalMethod = (() => SetDeathStepIndex(index))
                            };
                        Log($"🔧 Исправлен шаг {it}: теперь точка возрождения установлена на {nextIndex}");
                        break;
                }
            }
            else
            {
                Log($"❌ Ошибка: попытка исправить несуществующий шаг {it}");
            }
        }
        
        public int GetNextIndex()
        {
                
            var step = dict_step[_currentStepIndex];
            int nextIndex = step.GetNextIndex();
            _lastReturnedIndex = nextIndex;
            
            // Логируем информацию о переходе
            string stepType = step.GetType().Name;
            if (step is ConditionalRouteStep condStep)
            {
                bool conditionResult = condStep.Condition();
                Log($"🎲 Шаг {_currentStepIndex} [{stepType}]: условие = {conditionResult} -> переход к {nextIndex}");
            }
            else if (step is RouteStepWithMethod)
            {
                Log($"⚡ Шаг {_currentStepIndex} [{stepType}]: выполнение метода -> переход к {nextIndex}");
            }
            else
            {
                Log($"➡️ Шаг {_currentStepIndex} [{stepType}]: переход к {nextIndex}");
            }
            
            _currentStepIndex = nextIndex;
            
            return nextIndex;
        }
        
        public void Reset()
        {
            Log($"🔄 Сброс выполнения: текущий шаг {_currentStepIndex} -> стартовый {_startStepIndex}");
            _startStepIndex = 0;
            _currentStepIndex = _startStepIndex;
            _returnPoints.Clear();
            _deathStepIndex = -1;
        }

        public int getLastIndex()
        {
            return _lastReturnedIndex;
        }
        
        public int getDeathIndex()
        {
            return _deathStepIndex;
        }

        public int getReturnIndex()
        {
            return _returnPoints?.TryPeek(out var result) == true ? result : default;
        }
        
        private bool HasNext()
        {
            // Проверяем, остались ли еще шаги в маршруте
            bool hasNext = _currentStepIndex < dict_step.Count;
            if (!hasNext && _enableLogging)
            {
                Log($"📌 Маршрут завершен (шагов: {dict_step.Count})");
            }
            return hasNext;
        }

        private void SetStartStepIndex(int nextIndex)
        {
            _startStepIndex = nextIndex;
            Log($"⭐ Установлена точка старта: индекс {_startStepIndex}");
        }
        
        private void SetDeathStepIndex(int nextIndex)
        {
            _deathStepIndex = nextIndex;
            Log($"💀 Установлена точка смерти/возрождения: индекс {nextIndex}");
        }

        private void SetReturnStepIndex(int index)
        {
            _returnPoints.Push(index);
            Log($"📚 Добавлена точка возврата {index} в стек (глубина стека: {_returnPoints.Count})");
        }
        
        private void Log(string message)
        {
            if (_enableLogging)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [{_programmerName}] {message}");
            }
        }
        
        
        public List<int> BuildRoute()
        {
            Log("🚀 НАЧАЛО ПОСТРОЕНИЯ МАРШРУТА");
            var route = new List<int>();
            Reset();
            
            int stepCounter = 0;
            while (HasNext())
            {
                int nextIndex = GetNextIndex();
                route.Add(nextIndex);
                stepCounter++;
                
                // Защита от бесконечного цикла
                if (stepCounter > dict_step.Count * 10)
                {
                    Log($"⚠️ ПРЕРЫВАНИЕ: достигнут лимит шагов ({stepCounter}), возможно зацикливание");
                    break;
                }
            }
            
            Log($"✅ ПОСТРОЕНИЕ ЗАВЕРШЕНО: выполнено {route.Count} шагов");
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

        private class RouteStepUpdate : RouteStep
        {
            public Func<int> ExternalMethod { get; set; }
            
            public override int GetNextIndex()
            {
                NextIndex = ExternalMethod();
                return NextIndex;
            }
        }
    }
}