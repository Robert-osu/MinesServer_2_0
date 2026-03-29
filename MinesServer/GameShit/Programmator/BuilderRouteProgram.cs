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

        public void AddStep(int nextIndex)
        {
            _steps.Add(new RouteStep { NextIndex = nextIndex });
            Log($"Добавлен обычный шаг: переход к индексу {nextIndex}");
        }
        
        public void AddConditionalStep(Func<bool> condition, int trueIndex, int falseIndex)
        {
            _steps.Add(new ConditionalRouteStep
            {
                Condition = condition,
                TrueIndex = trueIndex,
                FalseIndex = falseIndex
            });
            Log($"Добавлен условный шаг: если условие истинно -> {trueIndex}, иначе -> {falseIndex}");
        }
        
        public void AddStartStep(int nextIndex, Action externalMethod = null)
        {
            _steps.Add(new RouteStepWithMethod
            {
                NextIndex = nextIndex,
                ExternalMethod = externalMethod ?? (() => SetStartStepIndex())
            });
            Log($"⭐ Добавлен стартовый шаг: переход к индексу {_currentStepIndex} (точка входа в программу)");
        }
        
        public void AddRecursiveStep(int nextIndex, int gotoIndex, Action externalMethod = null)
        {
            _steps.Add(new RouteStepWithMethod
            {
                NextIndex = gotoIndex,
                ExternalMethod = externalMethod ?? (() => SetReturnStepIndex(nextIndex))
            });
            Log($"🔄 Добавлен рекурсивный шаг: переход к {gotoIndex}, после возврата -> {nextIndex}");
        }
        
        public void AddReturnStep()
        {
            _steps.Add(new RouteStepUpdate { ExternalMethod = (() => _returnPoints.Pop()) });
            Log($"↩️ Добавлен шаг возврата: возвращаемся к индексу {_steps[_steps.Count - 1].NextIndex} (стек возврата: {_returnPoints.Count} элементов)");
            
        }
        
        public void AddEndStep()
        {
            _steps.Add(new RouteStepUpdate { ExternalMethod = (() => _startStepIndex) });
            Log($"🏁 Добавлен конечный шаг: возврат к стартовому индексу {_startStepIndex}");
        }
        
        public void AddDeathStep(int nextIndex, int gotoIndex, Action externalMethod = null)
        {
            _steps.Add(new RouteStepWithMethod
            {
                NextIndex = nextIndex,
                ExternalMethod = externalMethod ?? (() => SetDeathStepIndex(gotoIndex))
            });
            Log($"💀 Добавлен шаг смерти: при достижении индекса {nextIndex} активируется точка возрождения {gotoIndex}");
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
                        Log($"🔧 Исправлен шаг {it}: теперь переход к {nextIndex} (обычный)");
                        break;
                    case 1: // Command.CALL_FUNC
                        index = _steps[it].NextIndex;
                        _steps[it] = new RouteStepWithMethod
                            {
                                NextIndex = nextIndex,
                                ExternalMethod = (() => SetReturnStepIndex(index))
                            };
                        Log($"🔧 Исправлен шаг {it}: теперь вызов функции с возвратом к {nextIndex}");
                        break;
                    case 9: // Command.RESPAWN_TO
                        index = _steps[it].NextIndex;
                        _steps[it] = new RouteStepWithMethod
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
            if (_currentStepIndex >= _steps.Count)
            {
                Log($"⚠️ Достигнут конец маршрута! Возвращаемся к старту ({_startStepIndex})");
                _currentStepIndex = _startStepIndex;
            }
                
            var step = _steps[_currentStepIndex];
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
        
        private bool HasNext()
        {
            // Проверяем, остались ли еще шаги в маршруте
            bool hasNext = _currentStepIndex < _steps.Count;
            if (!hasNext && _enableLogging)
            {
                Log($"📌 Маршрут завершен (шагов: {_steps.Count})");
            }
            return hasNext;
        }

        private void SetStartStepIndex()
        {
            _startStepIndex = _currentStepIndex;
            Log($"⭐ Установлена точка старта: индекс {_currentStepIndex}");
        }
        
        private void SetDeathStepIndex(int index)
        {
            _deathStepIndex = index;
            Log($"💀 Установлена точка смерти/возрождения: индекс {index}");
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
        
        public void PrintRoute()
        {
            if (!_enableLogging) return;
            
            Console.WriteLine($"📋 ПОСТРОЕНИЕ МАРШРУТА ДЛЯ {_programmerName}");
            
            for (int i = 0; i < _steps.Count; i++)
            {
                var step = _steps[i];
                string stepInfo = step switch
                {
                    ConditionalRouteStep cs => $"УСЛОВИЕ → истина:{cs.TrueIndex}, ложь:{cs.FalseIndex}",
                    RouteStepWithMethod => $"С МЕТОДОМ → {step.NextIndex}",
                    _ => $"ОБЫЧНЫЙ → {step.NextIndex}"
                };
                
                string marker = i == _startStepIndex ? "⭐ СТАРТ" : 
                               i == _deathStepIndex ? "💀 СМЕРТЬ" : "   ";
                
                Console.WriteLine($"{marker} Шаг {i,3}: {stepInfo}");
            }
            
            Console.WriteLine($"\n📊 ИТОГО ШАГОВ: {_steps.Count}");
            Console.WriteLine($"📍 СТАРТОВЫЙ ИНДЕКС: {_startStepIndex}");
            Console.WriteLine($"💀 ИНДЕКС СМЕРТИ: {(_deathStepIndex >= 0 ? _deathStepIndex.ToString() : "не установлен")}");
            Console.WriteLine($"📚 ГЛУБИНА СТЕКА ВОЗВРАТА: {_returnPoints.Count}");
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
                if (stepCounter > _steps.Count * 10)
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