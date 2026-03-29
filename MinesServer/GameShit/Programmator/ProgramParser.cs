

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
        private void ParseRoute()
        {
            // создает route, добавляя шаги для каждого оператора
            int col_index = 0;
            int row_index = 0;
            int next = 0;
            string name = "";
            Dictionary<int, string> dict_from = new Dictionary<int, string>();
            Dictionary<string, int> dict_to = new Dictionary<string, int>(); // одинаковые метки перезаписываются
            Dictionary<int, int> dict_type = new Dictionary<int, int>();


            for (int i = 0; i < this.size; i++)
            {
                Command cmd = (Command)this.actions[i];
                
                switch (cmd)
                {
                    case Command.NEWLINE:
                        next = row_index * MaxActionsPerRow + MaxActionsPerRow; // начало некст строки
                        setDefaultStep(next, 0);
                        break;
                    case Command.GO_TO:
                        name = GetLabel(i);
                        dict_from[i] = name;
                        route.AddEndStep(); // заглушка - неверное имя ведет в точку старта
                        break;
                    case Command.START:
                        next = i + 1;
                        if (next < this.size && col_index + 1 < MaxActionsPerRow) // не конец проги и не конец строки
                        {
                            route.AddStartStep(next);
                        }
                        else
                        {
                            // зациклим на себе
                            route.AddStartStep(i);
                        }
                        break;
                    case Command.RETURN:
                        route.AddReturnStep(); // TODO
                        break;
                    case Command.RESPAWN_TO:
                        name = GetLabel(i);
                        dict_from[i] = name;
                        dict_type[i] = 9;
                        next = i + 1;
                        setDefaultStep(next, col_index);
                        break;
                    case Command.CALL_FUNC:
                    case Command.CALL_FUNC_CONDITION:
                    case Command.CALL_FUNC_STATE:
                        name = GetLabel(i);
                        dict_from[i] = name;
                        dict_type[i] = 1;
                        next = i + 1;
                        setDefaultStep(next, col_index); // заглушка - неверное имя ведет к пропуску процедуры
                        break;
                    case Command.RETURN_ARGUMENT:
                        route.AddReturnStep(); // TODO
                        break;
                    case Command.RETURN_STATE:
                        route.AddReturnStep(); // TODO
                        break;
                    case Command.YES_NO:
                        break;
                    case Command.YES_NO_NEWLINE:
                        break;
                    case Command.YES_NO_RETURN:
                        break;
                    case Command.YES_NO_START:
                        break;
                    case Command.YES_NO_STOP:
                        break;
                    case Command.NO_YES:
                        break;
                    case Command.NO_YES_NEWLINE:
                        break;
                    case Command.NO_YES_RETURN:
                        break;
                    case Command.NO_YES_START:
                        break;
                    case Command.NO_YES_STOP:
                        break;
                    default:
                        if (cmd == Command.LABEL)
                        {
                            name = GetLabel(i);
                            dict_to[name] = i;
                        }
                        next = i + 1;
                        setDefaultStep(next, col_index);
                        break;
                }

                col_index++;
                if (col_index >= MaxActionsPerRow)
                {
                    col_index = 0;
                    row_index++;
                }
            }


            FixGoTo(dict_from, dict_to, dict_type);
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

        private void setDefaultStep(int next, int col_index)
        {
            if (next < this.size && col_index + 1 < MaxActionsPerRow) // не конец проги и не конец строки
            {
                route.AddStep(next);
            }
            else
            {   // вернем в точку старта
                route.AddEndStep();
            }
            
        }
        
    }
}