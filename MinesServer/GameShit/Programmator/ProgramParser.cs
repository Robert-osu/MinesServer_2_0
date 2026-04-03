

using System.Text;
using MinesServer.GameShit.Entities.PlayerStaff;
using MinesServer.GameShit.Programmator.SevenZip.LZMA;

namespace MinesServer.GameShit.Programmator
{
    public class ProgramParser
    {
        public int id { get; set; }
        public string name { get; set; }
        public string data { get; set; }
        public Player owner { get; set; }
        private const int MaxActionsBytesSize = 4; // выделено 4 байта на определение количества команд (не изменять)
        private const int MaxActionsPerRow = 16; // максимальное количество команд в одной строке (не изменять)
        public RouteBuilder route = new();
        public int size;
        public byte[] actions;
        public string[] labels;
        public ProgramParser(Player P, string name, string data)
        {
            this.owner = P;
            this.name = name;
            this.data = data;

            ParseData();
            ParseRoute();
        }

        private void ParseData()
        {
            byte[] array = SevenZipHelper.Decompress(Convert.FromBase64String(data)); // наша программа в виде массива байтов
            int commands_size = BitConverter.ToInt32(array, 0); // количество команд в программе
            this.size = commands_size;

            // actions_array - содержит массив команд
            var start_byte = MaxActionsBytesSize; // сдвиг
            var end_byte = commands_size + start_byte;
            //byte[] actions_array = GetBytesRange(array, start_byte, end_byte);
            this.actions = GetBytesRange(array, start_byte, end_byte);

            // for (int i = 0; i < commands_size; i++)
            // {
            //     Console.Write(actions_array[i]);
            // }

            // labels_array - содержит массив значений команд, либо два значения, если передаются как "WWW@10"
            start_byte = commands_size + MaxActionsBytesSize; // сдвиг
            end_byte = array.Length - start_byte;
            var labels_array = Encoding.UTF8.GetString(array, start_byte, end_byte).Split(':');
            this.labels = Encoding.UTF8.GetString(array, start_byte, end_byte).Split(':');
        }
        
        
        private Dictionary<int, string> dict_from = new Dictionary<int, string>();
        private Dictionary<string, int> dict_to = new Dictionary<string, int>(); // одинаковые метки перезаписываются
        private Dictionary<int, int> dict_type = new Dictionary<int, int>();
        
        // private void ParseRoute2()
        // {
        //     // создает route, добавляя шаги для каждого оператора
        //     int col_index = 0;
        //     int row_index = 0;
        //     int next = 0;
        //     string name = "";


        //     for (int i = 0; i < this.size; i++)
        //     {
        //         Command cmd = (Command)this.actions[i];
                
        //         switch (cmd)
        //         {
        //             case Command.NEWLINE:
        //                 next = row_index * MaxActionsPerRow + MaxActionsPerRow; // начало некст строки
        //                 setDefaultStep(next, 0);
        //                 break;
        //             case Command.GO_TO:
        //                 name = GetLabel(i);
        //                 dict_from[i] = name;
        //                 route.AddEndStep(); // заглушка - неверное имя ведет в точку старта
        //                 break;
        //             case Command.START:
        //                 next = i + 1;
        //                 if (next < this.size && col_index + 1 < MaxActionsPerRow) // не конец проги и не конец строки
        //                 {
        //                     route.AddStartStep(next);
        //                 }
        //                 else
        //                 {
        //                     // зациклим на себе
        //                     route.AddStartStep(i);
        //                 }
        //                 break;
        //             case Command.RETURN:
        //                 route.AddReturnStep(); // TODO
        //                 break;
        //             case Command.RESPAWN_TO:
        //                 name = GetLabel(i);
        //                 dict_from[i] = name;
        //                 dict_type[i] = 9;
        //                 next = i + 1;
        //                 setDefaultStep(next, col_index);
        //                 break;
        //             case Command.CALL_FUNC:
        //             case Command.CALL_FUNC_CONDITION:
        //             case Command.CALL_FUNC_STATE:
        //                 name = GetLabel(i);
        //                 dict_from[i] = name;
        //                 dict_type[i] = 1;
        //                 next = i + 1;
        //                 setDefaultStep(next, col_index); // заглушка - неверное имя ведет к пропуску процедуры
        //                 break;
        //             case Command.RETURN_ARGUMENT:
        //                 route.AddReturnStep(); // TODO
        //                 break;
        //             case Command.RETURN_STATE:
        //                 route.AddReturnStep(); // TODO
        //                 break;
        //             case Command.YES_NO:
        //             case Command.YES_NO_NEWLINE:
        //             case Command.YES_NO_RETURN:
        //             case Command.YES_NO_START:
        //             case Command.YES_NO_STOP:
        //             case Command.NO_YES:
        //             case Command.NO_YES_NEWLINE:
        //             case Command.NO_YES_RETURN:
        //             case Command.NO_YES_START:
        //             case Command.NO_YES_STOP:
        //             default:
        //                 if (cmd == Command.LABEL)
        //                 {
        //                     name = GetLabel(i);
        //                     dict_to[name] = i;
        //                 }
        //                 next = i + 1;
        //                 setDefaultStep(next, col_index);
        //                 break;
        //         }

        //         col_index++;
        //         if (col_index >= MaxActionsPerRow)
        //         {
        //             col_index = 0;
        //             row_index++;
        //         }
        //     }


        //     FixGoTo(dict_from, dict_to, dict_type);
        //     route.Reset();
        // }
        
        private void ParseRoute()
        {
            // задача: задать всем элементам control_flow четкие переходы
            HashSet<byte> searchSet = CommandExtensions.CONTROL_FLOW.ToByteHashSet();
            var indexes = FindIndexesFromSet(actions, searchSet);
            var type = Command.EMPTY;
            var _ind = 0;
            var name = "";

            // здесь мы заполнили dict_from - откуда джампаем
            // for (int i = 0; i < indexes.Count; i++)
            // {
            //     _ind = indexes[i];
            //     type = (Command)actions[_ind];
            //     name = GetLabel(_ind);

            //     switch (type)
            //     {
            //         case Command.GO_TO:
            //         case Command.RESPAWN_TO: 
            //         case Command.CALL_FUNC:
            //         case Command.CALL_FUNC_CONDITION:
            //         case Command.CALL_FUNC_STATE:
            //         case Command.YES_NO:
            //         case Command.NO_YES:
            //             dict_type.Add(_ind, (byte)type);
            //             dict_from.Add(_ind, name);
            //             break;
            //     }
            // }
        

            searchSet.Clear();
            searchSet.Add((byte)Command.LABEL);
            indexes = FindIndexesFromSet(actions, searchSet);
        
            // заполнили dict_to - куда джампаем
            for (int i = 0; i < indexes.Count; i++)
            {
                _ind = indexes[i];
                name = GetLabel(_ind);
                dict_type.Add(_ind, (byte)type);
                dict_to.Add(name, _ind);
            }

            var i_current = 0;
            HashSet<int> visited = new();
            var i_col = 0;
            var i_row = 0;
            var i_start = 0;
            var last_index = 0;
            CommandsConnector(i_current, visited, i_col, i_row, i_start, last_index);
        }

        private void CommandsConnector(int index, HashSet<int> visited, int i_col, int i_row, int i_start, int last_index)
        {   // рекурсивно обходит рабочие операторы программы
            
            Console.WriteLine($"[DEBUG] Entering CommandsConnector: index={index}, row={i_row}, col={i_col}, start={i_start}");
            
            if (index >= size || visited.Contains(index))
            {
                Console.WriteLine($"[DEBUG] Termination condition: index={index}, visited={visited.Contains(index)}");
                return;
            }
            
            visited.Add(index);
            Console.WriteLine($"[DEBUG] Added index {index} to visited. Total visited: {visited.Count}");

            // создает route, добавляя шаги для каждого оператора
            int next = 0;
            int goto_index = 0;
            string name = "";
            Command cmd = (Command)this.actions[index];
            next = index + 1;

            if (route._isFirstStep)
            {
                i_start = index;
            }
            
            Console.WriteLine($"[DEBUG] Processing command: {cmd} at index {index}");
            
            switch (cmd)
            {
                case Command.EMPTY:
                    if (next < this.size && i_col + 1 < MaxActionsPerRow) // не конец проги и не конец строки
                    {
                        route.AddTravel(last_index);
                        Console.WriteLine($"[DEBUG] {cmd}: passed step {index} -> {next}");
                    }
                    else
                    {
                        route.AddBreak(index, i_start);
                        next = i_start;
                        Console.WriteLine($"[DEBUG] {cmd}: end of line, returning to start {i_start}");
                    }
                    break;
                case Command.NEWLINE:
                    route.AddTravel(last_index);
                    Console.WriteLine($"[DEBUG] NEWLINE: jumping to next row start");
                    next = i_row * MaxActionsPerRow + MaxActionsPerRow; // начало некст строки
                    if (next < this.size) // не конец проги и не конец строки
                    {
                        route.AddStep(index, next);
                        Console.WriteLine($"[DEBUG] NEWLINE: added step {index} -> {next}");
                    }
                    else
                    {   // вернем в точку старта
                        route.AddEndStep(index);
                        next = i_start;
                        Console.WriteLine($"[DEBUG] NEWLINE: end of program, returning to start {i_start}");
                    }
                    break;
                case Command.GO_TO:
                    route.AddTravel(last_index);
                    name = GetLabel(index);
                    Console.WriteLine($"[DEBUG] GO_TO: looking for label '{name}'");
                    if (dict_to.TryGetValue(name, out next))
                    {
                        route.AddStep(index, next);
                        Console.WriteLine($"[DEBUG] GO_TO: found label, step {index} -> {next}");
                    }
                    else
                    {
                        route.AddEndStep(index);
                        next = i_start;
                        Console.WriteLine($"[DEBUG] GO_TO: label not found, returning to start {i_start}");
                    }
                    break;
                case Command.START:
                    route.AddTravel(last_index);
                    Console.WriteLine($"[DEBUG] START: setting start point");
                    if (next < this.size && i_col + 1 < MaxActionsPerRow) // не конец проги и не конец строки
                    {
                        route.AddStartStep(index, next);
                        i_start = next;
                        Console.WriteLine($"[DEBUG] START: added start step {index} -> {next}, new start={next}");
                    }
                    else
                    {
                        // зациклим на себе
                        route.AddStartStep(index, index);
                        i_start = index;
                        next = i_start;
                        Console.WriteLine($"[DEBUG] START: end of line, looping on self {index}");
                    }
                    break;
                case Command.RESPAWN_TO:
                    route.AddTravel(last_index);
                    name = GetLabel(index);
                    Console.WriteLine($"[DEBUG] RESPAWN_TO: looking for label '{name}'");
                    if (dict_to.TryGetValue(name, out goto_index))
                    {
                        if (next < this.size && i_col + 1 < MaxActionsPerRow) // не конец проги и не конец строки
                        {
                            route.AddDeathStep(index, next, goto_index);
                            Console.WriteLine($"[DEBUG] RESPAWN_TO: added death step {index} -> {next}, respawn={goto_index}");
                        }
                        else
                        {
                            route.AddDeathStep(index, i_start, goto_index);
                            next = i_start;
                            Console.WriteLine($"[DEBUG] RESPAWN_TO: end of line, death step to start={i_start}, respawn={goto_index}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG] RESPAWN_TO: label not found, treating as normal step");
                        if (next < this.size && i_col + 1 < MaxActionsPerRow) // не конец проги и не конец строки
                        {
                            route.AddStep(index, next);
                            Console.WriteLine($"[DEBUG] RESPAWN_TO: added step {index} -> {next}");
                        }
                        else
                        {
                            route.AddEndStep(index);
                            next = i_start;
                            Console.WriteLine($"[DEBUG] RESPAWN_TO: end of line, returning to start {i_start}");
                        }
                    }
                    break;
                case Command.CALL_FUNC:
                case Command.CALL_FUNC_CONDITION:
                case Command.CALL_FUNC_STATE:
                    route.AddTravel(last_index);
                    name = GetLabel(index);
                    Console.WriteLine($"[DEBUG] {cmd}: calling function '{name}'");
                    if (dict_to.TryGetValue(name, out goto_index))
                    {
                        if (next < this.size && i_col + 1 < MaxActionsPerRow) // не конец проги и не конец строки
                        {
                            route.AddRecursiveStep(index, next, goto_index);
                            Console.WriteLine($"[DEBUG] {cmd}: added recursive step {index} -> {next}, function={goto_index}");
                        }
                        else
                        {
                            route.AddRecursiveStep(index, i_start, goto_index);
                            next = i_start;
                            Console.WriteLine($"[DEBUG] {cmd}: end of line, recursive step to start={i_start}, function={goto_index}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG] {cmd}: function not found, treating as normal step");
                        if (next < this.size && i_col + 1 < MaxActionsPerRow) // не конец проги и не конец строки
                        {
                            route.AddStep(index, next);
                            Console.WriteLine($"[DEBUG] {cmd}: added step {index} -> {next}");
                        }
                        else
                        {
                            route.AddEndStep(index);
                            next = i_start;
                            Console.WriteLine($"[DEBUG] {cmd}: end of line, returning to start {i_start}");
                        }
                    }
                    break;
                case Command.RETURN:
                case Command.RETURN_ARGUMENT:
                case Command.RETURN_STATE:
                    route.AddTravel(last_index);
                    next = route.getReturnIndex();
                    route.AddReturnStep(index); // TODO
                    Console.WriteLine($"[DEBUG] {cmd}: added return step, returning to index {next}");
                    break;
                case Command.YES_NO:
                case Command.NO_YES:
                    route.AddTravel(last_index);
                    name = GetLabel(index);
                    Console.WriteLine($"[DEBUG] {cmd}: conditional with label '{name}'");
                    if (dict_to.TryGetValue(name, out goto_index))
                    {
                        if (next < this.size && i_col + 1 < MaxActionsPerRow) // не конец проги и не конец строки
                        {
                            route.AddConditionalStep(index, (() => true), next, goto_index);
                            Console.WriteLine($"[DEBUG] {cmd}: added conditional step {index} -> {next} (true) / {goto_index} (false)");
                        }
                        else
                        {
                            route.AddConditionalStep(index, (() => true), i_start, goto_index);
                            next = i_start;
                            Console.WriteLine($"[DEBUG] {cmd}: end of line, conditional to start={i_start}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"[DEBUG] {cmd}: label not found, using start={i_start} as fallback");
                        if (next < this.size && i_col + 1 < MaxActionsPerRow) // не конец проги и не конец строки
                        {
                            route.AddConditionalStep(index, (() => true), next, i_start);
                            Console.WriteLine($"[DEBUG] {cmd}: added conditional step {index} -> {next} / {i_start}");
                        }
                        else
                        {
                            route.AddEndStep(index);
                            next = i_start;
                            Console.WriteLine($"[DEBUG] {cmd}: end of line, returning to start {i_start}");
                        }
                    }
                    break;
                case Command.YES_NO_NEWLINE:
                case Command.YES_NO_RETURN:
                case Command.YES_NO_START:
                case Command.YES_NO_STOP:
                case Command.NO_YES_NEWLINE:
                case Command.NO_YES_RETURN:
                case Command.NO_YES_START:
                case Command.NO_YES_STOP:
                default:
                    Console.WriteLine($"[DEBUG] {cmd}: default processing (simple step)");
                    if (next < this.size && i_col + 1 < MaxActionsPerRow) // не конец проги и не конец строки
                    {
                        route.AddStep(index, next);
                        Console.WriteLine($"[DEBUG] {cmd}: added step {index} -> {next}");
                    }
                    else
                    {
                        route.AddEndStep(index);
                        next = i_start;
                        Console.WriteLine($"[DEBUG] {cmd}: end of line, returning to start {i_start}");
                    }
                    last_index = index;
                    break;
            }

            i_col++;
            if (i_col >= MaxActionsPerRow)
            {
                i_col = 0;
                i_row++;
                Console.WriteLine($"[DEBUG] Moved to next row: row={i_row}, col={i_col}");
            }

            Console.WriteLine($"[DEBUG] Recursive call: next={next}, row={i_row}, col={i_col}, start={i_start}");

            i_row = next / MaxActionsPerRow;
            i_col = next % MaxActionsPerRow;
            CommandsConnector(next, visited, i_col, i_row, i_start, last_index); // следующий вызов

            if (!visited.Contains(goto_index))
            {
                // Определение строки и колонки по индексу
                i_row = goto_index / MaxActionsPerRow;
                i_col = goto_index % MaxActionsPerRow;
                CommandsConnector(goto_index, visited, i_col, i_row, i_start, last_index); // ответвление
            }
        }

        public static List<int> FindIndexesFromSet(byte[] array, HashSet<byte> searchSet)
        {
            var indexes = new List<int>();
            
            for (int i = 0; i < array.Length; i++)
            {
                if (searchSet.Contains(array[i]))
                {
                    indexes.Add(i);
                }
            }
            
            return indexes;
        }

        private void FixGoTo(Dictionary<int, string> from, Dictionary<string, int> to, Dictionary<int, int> _type = null)
        {   // правит переходы, не были заполнены сразу
            int next = -1; // next может быть нулем
            int type = 0; // default для простого шага
            foreach (var pair in from)
            {
                next = to.ContainsKey(pair.Value) ? to[pair.Value] : -1;
                
                if (next >= 0)
                {
                    type = _type.ContainsKey(pair.Key) ? _type[pair.Key] : 0;
                    route.FixStep(pair.Key, next, type);
                } 
            }
        }

        private byte[] GetBytesRange(byte[] array, int startByte, int endByte)
        {
            int length = endByte - startByte;
            byte[] result = new byte[length];
            Array.Copy(array, startByte, result, 0, length);
            return result;
        }
        public string GetLabel(int index, bool isSecondLabel = false)
        { // до 100к операций пофиг
            if (index >= this.labels.Count())
            {
                return "0";
            }
            var parts = this.labels[index]?.Split('@') ?? new[] { "0" };

            if (isSecondLabel && parts.Length > 1)
                return parts[1]; // после @
            else
                return parts[0]; // до @ или вся строка
        }
        
    }
}