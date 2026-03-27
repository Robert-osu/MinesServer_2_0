

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
        private const int MaxRows = 12; // максимальное количество команд в одной строке (не изменять)
        private const int MaxPages = 16; // максимальное количество страниц (не изменять)
        private RouteBuilder route = new();
        private int size;
        private byte[] actions;
        private string[] labels;
        private HashSet<int> processedIndexes = new();
        public ProgramManager(Player P, string name, string data)
        {
            this.owner = P;
            this.name = name;
            this.data = data;

            ParseData();
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
            for (int i = 0; i < this.actions.Count(); i++)
            {
                Command cmd = (Command)this.actions[i];
                if (CommandExtensions.CONTROL_FLOW.Contains(cmd))
                {
                    
                }
                else
                {
                    
                }
            }
        }
        private void ParseRow()
        {
            
        }


        private byte[] GetBytesRange(byte[] array, int startByte, int endByte)
        {
            int length = endByte - startByte;
            byte[] result = new byte[length];
            Array.Copy(array, startByte, result, 0, length);
            return result;
        }
        private string GetLabel(int index, bool isSecondLabel = false)
        { // до 100к операций пофиг
            var parts = this.labels[index]?.Split('@') ?? new[] { "0" };

            if (isSecondLabel && parts.Length > 1)
                return parts[1]; // после @
            else
                return parts[0]; // до @ или вся строка
        }

        
    }
}