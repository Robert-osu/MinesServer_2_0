using MinesServer.GameShit.Entities.PlayerStaff;
using MinesServer.GameShit.Programmator.SevenZip.LZMA;
using System.Diagnostics;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using MinesServer.GameShit.Programmator;

namespace MinesServer.GameShit.Programmator
{
    public class Program
    {
        private const int MaxActionsBytesSize = 4; // выделено 4 байта на определение количества команд (не изменять)
        private const int MaxActionsPerRow = 16; // максимальное количество команд в одной строке (не изменять)
        public RouteBuilder route = new();
        private Program()
        {

        }
        public Program(Player P,string name,string data)
        {
            owner = P;
            this.name = name;this.data = data;
        }
        public int id { get; set; }
        public string name { get; set; }
        public string data { get; set; }
        public Player owner { get; set; }
        public Dictionary<string, PFunction> programm
        {
            get
            {
                _programm ??= parseNormal();
                return _programm;
            }
        }
        private Dictionary<string,PFunction> parseNormal()
        {
            bool isStart = false;

            Dictionary<string, PFunction> functions = new();
            functions[""] = new PFunction();
            string name_current_f = "";

            byte[] array = SevenZipHelper.Decompress(Convert.FromBase64String(data)); // наша программа в виде массива байтов
            int commands_size = BitConverter.ToInt32(array, 0); // количество команд в программе

            // actions_array - содержит массив команд
            var start_byte = MaxActionsBytesSize; // сдвиг
            var end_byte = commands_size + start_byte;
            byte[] actions_array = GetBytesRange(array, start_byte, end_byte);

            for (int i = 0; i < commands_size; i++)
            {
                Console.Write(actions_array[i]);
            }

            // labels_array - содержит массив значений команд, либо два значения, если передаются как "WWW@10"
            start_byte = commands_size + MaxActionsBytesSize; // сдвиг
            end_byte = array.Length - start_byte;
            var labels_array = Encoding.UTF8.GetString(array, start_byte, end_byte).Split(':');

            bool have_next_row = false; // есть ли переход на новую строку
            int i_column = 0;
            for (int i = 0; i < commands_size; i++)
            {
                var c_byte = Convert.ToInt16(array[i + 4]); // два байта? что?
                var action_type = CommandExtensions.GetByIndex(c_byte);
                var atype = GetActionType(c_byte); // для совместимости (удалить)

                var label_name = "0";
                var label_name2 = 0; // TODO: делать ли из целочисленного - строку?
                if (labels_array.Length > i)
                {
                    if (labels_array[i].Contains('@'))
                    {
                        var values = labels_array[i].Split('@');
                        label_name = values[0];
                        if (int.TryParse(values[1], out var value))
                            label_name2 = value;
                    }
                    else
                        label_name = labels_array[i];
                }

                if (CommandExtensions.NO_ARGS.Contains(action_type))
                {


                    switch (action_type)
                    {
                        case Command.EMPTY:
                            i_column++;
                            continue;
                        case Command.NEWLINE:
                            Console.Write("newline");
                            have_next_row = true;
                            // Сбрасываем счетчик строки
                            i_column = 0;
                            continue; // Пропускаем добавление команды
                        case Command.START:
                            isStart = true;
                            continue;
                        default:
                            if (CommandExtensions.CONDITIONS.Contains(action_type) ||
                                CommandExtensions.OFFSETS.Contains(action_type) ||
                                CommandExtensions.CHECKS.Contains(action_type))
                            {
                                continue;
                            }
                            else if (CommandExtensions.ACTIONS.Contains(action_type))
                            {
                                if(isStart)
                                {
                                    route.AddStartStep(i);
                                    isStart = false;
                                }
                                else
                                {
                                    route.AddStep(i);
                                }
                                Console.Write("+");
                                functions[name_current_f] += new PAction(atype);
                            }
                            break;

                    }
                } 
                else if (CommandExtensions.ONE_ARGS.Contains(action_type))
                {
                    if (action_type == Command.GO_TO)
                    {
                        // TODO: логика перехода GOTO
                    }
                    functions[name_current_f] += new PAction(atype, label_name);
                }
                else if (CommandExtensions.TWO_ARGS.Contains(action_type))
                {
                    functions[name_current_f] += new PAction(atype, label_name, label_name2);
                }
                else
                {
                    if (atype != ActionType.None)
                        {
                            Console.WriteLine($"Unknown action ID: {c_byte}");
                            functions[name_current_f] += new PAction(atype);
                        }
                }

                i_column++;

                if (i_column >= MaxActionsPerRow)
                {
                    if (have_next_row)
                    {
                        have_next_row = false;
                    }
                    else
                    {
                        Console.Write("end");
                        return functions;
                    }
                    i_column = 0;
                }
            }
            return functions;
        }
        byte[] GetBytesRange(byte[] array, int startByte, int endByte)
        {
            int length = endByte - startByte;
            byte[] result = new byte[length];
            Array.Copy(array, startByte, result, 0, length);
            return result;
        }
        public static ActionType GetActionType(int id)
        {
            return id switch
            {
                0 => ActionType.None,
                1 => ActionType.NextRow,
                2 => ActionType.Start,
                3 => ActionType.Stop,
                4 => ActionType.MoveUp,
                5 => ActionType.MoveLeft,
                6 => ActionType.MoveDown,
                7 => ActionType.MoveRight,
                8 => ActionType.Dig,
                9 => ActionType.RotateUp,
                10 => ActionType.RotateLeft,
                11 => ActionType.RotateDown,
                12 => ActionType.RotateRight,
                13 => ActionType.Last,                    // LAST
                14 => ActionType.MoveForward,
                15 => ActionType.RotateLeftRelative,
                16 => ActionType.RotateRightRelative,
                17 => ActionType.BuildBlock,
                18 => ActionType.Geology,
                19 => ActionType.BuildRoad,
                20 => ActionType.Heal,
                21 => ActionType.BuildPillar,
                22 => ActionType.RotateRandom,
                23 => ActionType.Beep,
                24 => ActionType.GoTo,
                25 => ActionType.RunSub,
                26 => ActionType.RunFunction,
                27 => ActionType.Return,
                28 => ActionType.ReturnFunction,
                29 => ActionType.CheckUpLeft,
                30 => ActionType.CheckDownRight,
                31 => ActionType.CheckUp,
                32 => ActionType.CheckUpRight,
                33 => ActionType.CheckLeft,
                35 => ActionType.CheckRight,
                36 => ActionType.CheckDownLeft,
                37 => ActionType.CheckDown,
                38 => ActionType.Or,
                39 => ActionType.And,
                40 => ActionType.CreateFunction,
                43 => ActionType.IsNotEmpty,
                44 => ActionType.IsEmpty,
                45 => ActionType.IsFalling,
                46 => ActionType.IsCrystal,
                47 => ActionType.IsLivingCrystal,
                48 => ActionType.IsBoulder,
                49 => ActionType.IsSand,
                50 => ActionType.IsBreakableRock,
                51 => ActionType.IsUnbreakable,
                52 => ActionType.IsRedRock,
                53 => ActionType.IsBlackRock,
                54 => ActionType.IsAcid,
                57 => ActionType.IsQuadBlock,
                58 => ActionType.IsRoad,
                59 => ActionType.IsRedBlock,
                60 => ActionType.IsYellowBlock,
                74 => ActionType.IsBox,
                76 => ActionType.IsPillar,
                77 => ActionType.IsGreenBlock,
                119 => ActionType.WritableStateMore,
                120 => ActionType.WritableStateLower,
                123 => ActionType.WritableState,
                131 => ActionType.ShiftUp,
                132 => ActionType.ShiftLeft,
                133 => ActionType.ShiftDown,
                134 => ActionType.ShiftRight,
                135 => ActionType.CheckForward,
                136 => ActionType.ShiftForward,
                137 => ActionType.RunState,
                138 => ActionType.ReturnState,
                139 => ActionType.RunIfFalse,
                140 => ActionType.RunIfTrue,
                141 => ActionType.MacrosDig,
                142 => ActionType.MacrosBuild,
                143 => ActionType.MacrosHeal,
                144 => ActionType.Flip,
                145 => ActionType.MacrosMine,
                146 => ActionType.CheckGun,
                147 => ActionType.FillGun,
                148 => ActionType.IsHpLower100,
                149 => ActionType.IsHpLower50,
                156 => ActionType.CheckForwardLeft,
                157 => ActionType.CheckForwardRight,
                158 => ActionType.EnableAutoDig,
                159 => ActionType.DisableAutoDig,
                160 => ActionType.EnableAgression,
                161 => ActionType.DisableAgression,
                162 => ActionType.BOOM,                    // ACTION_BOOM
                163 => ActionType.DISCHARGE,               // ACTION_DISCHARGE
                164 => ActionType.PROTON,                  // ACTION_PROTON
                165 => ActionType.VB,                      // ACTION_WB
                166 => ActionType.RunOnRespawn,
                167 => ActionType.Geopack,                  // ACTION_GEOPACK
                168 => ActionType.ZZ,                       // ACTION_ZZ
                169 => ActionType.C190,                     // ACTION_C190
                170 => ActionType.Poly,                      // ACTION_POLY
                171 => ActionType.Up,                        // ACTION_UP
                172 => ActionType.Craft,                     // ACTION_CRAFT
                173 => ActionType.Nano,                      // ACTION_NANO
                174 => ActionType.Rembot,                    // ACTION_REMBOT
                175 => ActionType.InvDirUp,                  // INVDIR_W
                176 => ActionType.InvDirLeft,                // INVDIR_A
                177 => ActionType.InvDirDown,                // INVDIR_S
                178 => ActionType.InvDirRight,               // INVDIR_D
                179 => ActionType.EnableHandMode,            // HANDMODE_ON
                180 => ActionType.DisableHandMode,           // HANDMODE_OFF
                181 => ActionType.DebugBreak,                // DEBUG_BREAK
                182 => ActionType.DebugSet,                  // DEBUG_SET
                200 => ActionType.Restart,                    // RESTART
                _ => ActionType.None
            };
        }
        private Dictionary<string, PFunction> _programm;
    }
}
