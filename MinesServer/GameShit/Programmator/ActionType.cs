
using System;
using System.Collections.Generic;
using System.Linq;

namespace MinesServer.GameShit.Programmator
{
    public enum ActionType
    {
        None,
        MoveUp,
        MoveLeft,
        MoveDown,
        MoveRight,
        MoveForward,
        RotateUp,
        RotateLeft,
        RotateDown,
        RotateRight,
        RotateLeftRelative,
        RotateRightRelative,
        RotateRandom,
        Dig,
        BuildBlock,
        BuildPillar,
        BuildRoad,
        BuildMilitaryBlock,
        Geology,
        Heal,
        NextRow,
        CreateFunction,
        GoTo,
        WritableStateMore,
        WritableStateLower,
        WritableState,
        RunSub,
        RunFunction,
        RunState,
        RunOnRespawn,
        RunIfTrue,
        RunIfFalse,
        Return,
        ReturnFunction,
        ReturnState,
        Start,
        Stop,
        Beep,
        CheckUp,
        CheckLeft,
        CheckDown,
        CheckRight,
        CheckUpLeft,
        CheckUpRight,
        CheckDownLeft,
        CheckDownRight,
        CheckForward,
        CheckForwardLeft,
        CheckForwardRight,
        CheckLeftRelative,
        CheckRightRelative,
        ShiftUp,
        ShiftLeft,
        ShiftDown,
        ShiftRight,
        ShiftForward,
        EnableAgression,
        DisableAgression,
        EnableAutoDig,
        DisableAutoDig,
        Flip,
        MacrosDig,
        MacrosBuild,
        MacrosHeal,
        MacrosMine,
        Or,
        And,
        IsHpLower100,
        IsHpLower50,
        IsNotEmpty,
        IsEmpty,
        IsFalling,
        IsCrystal,
        IsLivingCrystal,
        IsBoulder,
        IsSand,
        IsBreakableRock,
        IsUnbreakable,
        IsAcid,
        IsRedRock,
        IsBlackRock,
        IsGreenBlock,
        IsYellowBlock,
        IsRedBlock,
        IsPillar,
        IsQuadBlock,
        IsRoad,
        IsBox,
        CheckGun,
        FillGun,
        Last,
        BOOM,
        DISCHARGE,
        PROTON,
        VB,
        Geopack,
        ZZ,
        C190,
        Poly,
        Up,
        Craft,
        Nano,
        Rembot,
        InvDirUp,
        InvDirLeft,
        InvDirDown,
        InvDirRight,
        EnableHandMode,
        DisableHandMode,
        DebugBreak,
        DebugSet,
        Restart
    }


    public enum Command : byte
    {
        EMPTY = 0x00,
        NEWLINE = 0x01,
        START = 0x02,
        STOP = 0x03,
        MOVE_TOP = 0x04,
        MOVE_LEFT = 0x05,
        MOVE_BOTTOM = 0x06,
        MOVE_RIGHT = 0x07,
        DIG = 0x08,
        DIR_TOP = 0x09,
        DIR_LEFT = 0x0A,
        DIR_BOTTOM = 0x0B,
        DIR_RIGHT = 0x0C,
        REPEAT = 0x0D,
        MOVE_FORWARD = 0x0E,
        ROTATE_LEFT = 0x0F,
        ROTATE_RIGHT = 0x10,
        BUILD_BLOCK = 0x11,
        GEO = 0x12,
        BUILD_ROAD = 0x13,
        HEAL = 0x14,
        BUILD_QUADRO = 0x15,
        DIR_RANDOM = 0x16,
        ALARM = 0x17,
        GO_TO = 0x18,
        CALL_FUNC = 0x19,
        CALL_FUNC_CONDITION = 0x1A,
        RETURN = 0x1B,
        RETURN_ARGUMENT = 0x1C,
        CHECK_TOP_LEFT = 0x1D,
        CHECK_BOTTOM_RIGHT = 0x1E,
        CHECK_TOP = 0x1F,
        CHECK_TOP_RIGHT = 0x20,
        CHECK_LEFT = 0x21,
        CHECK_CENTER = 0x22,
        CHECK_RIGHT = 0x23,
        CHECK_BOTTOM_LEFT = 0x24,
        CHECK_BOTTOM = 0x25,
        OR = 0x26,
        AND = 0x27,
        LABEL = 0x28,
        YES_NO_RETURN = 0x29,
        NO_YES_RETURN = 0x2A,
        IS_NOT_EMPTY = 0x2B,
        IS_EMPTY = 0x2C,
        IS_FALLING = 0x2D,
        IS_CRYSTAL = 0x2E,
        IS_ALIVE_CRYSTAL = 0x2F,
        IS_FALLING_GRAVEL = 0x30,
        IS_FALLING_SAND = 0x31,
        IS_BREAKABLE = 0x32,
        IS_UNBREAKABLE = 0x33,
        IS_KRASNOSKAL = 0x34,
        IS_CHERNOSKAL = 0x35,
        IS_SLIME = 0x36,
        IS_ERROR1 = 0x37,
        IS_SAND_YB = 0x38,
        IS_QUADRO = 0x39,
        IS_ROAD = 0x3A,
        IS_RED_BLOCK = 0x3B,
        IS_YELLOW_BLOCK = 0x3C,
        IS_HP_LESS_90 = 0x3D,
        IS_HP_LESS_50 = 0x3E,
        IS_KISLOTKA = 0x3F,
        IS_COMMON_GRAVEL = 0x40,
        IS_LAVA = 0x41,
        IS_ALIVE_CYAN = 0x42,
        IS_ALIVE_WHITE = 0x43,
        IS_ALIVE_RED = 0x44,
        IS_ALIVE_VIOLET = 0x45,
        IS_ALIVE_BLACK = 0x46,
        IS_ALIVE_BLUE = 0x47,
        IS_ALIVE_RAINBOW = 0x48,
        IS_ERROR2 = 0x49,
        IS_BOX = 0x4A,
        IS_ERROR3 = 0x4B,
        IS_OPORA = 0x4C,
        IS_GREEN_BLOCK = 0x4D,
        IS_FULL = 0x4E,
        IS_FULL_GEO = 0x4F,
        IS_ERROR4 = 0x50,
        AFTER_RESPAWN = 0x51,
        AFTER_DAMAGE = 0x52,
        AFTER_ROBOTS = 0x53,
        IS_ERROR5 = 0x54,
        IS_ERROR6 = 0x55,
        OFFSET_LEFT_HAND = 0x56,
        OFFSET_RIGHT_HAND = 0x57,
        OFFSET_BEHIND = 0x58,
        BOX_ALL = 0x59,
        BOX_HALF = 0x5A,
        BOX_WHITE = 0x5B,
        BOX_GREEN = 0x5C,
        BOX_RED = 0x5D,
        BOX_BLUE = 0x5E,
        BOX_CYAN = 0x5F,
        BOX_VIOLET = 0x60,
        SET = 0x61,
        GET = 0x62,
        SET_VALUE = 0x63,
        ADD_VALUE = 0x64,
        MULTIPLY_VALUE = 0x65,
        DIVIDE_VALUE = 0x66,
        SUBTRACT_VALUE = 0x67,
        ADD = 0x68,
        MULTIPLY = 0x69,
        DIVIDE = 0x6A,
        SUBTRACT = 0x6B,
        ADD_VAR = 0x6C,
        MULTIPLY_VAR = 0x6D,
        DIVIDE_VAR = 0x6E,
        SUBTRACT_VAR = 0x6F,
        IS_GREATER = 0x70,
        IS_LESS = 0x71,
        IS_GREATER_OR_EQUAL = 0x72,
        IS_LESS_OR_EQUAL = 0x73,
        IS_EQUAL = 0x74,
        IS_NOT_EQUAL = 0x75,
        IS_ERROR7 = 0x76,
        VAR_GREATER = 0x77,
        VAR_LESS = 0x78,
        VAR_GREATER_OR_EQUAL = 0x79,
        VAR_LESS_OR_EQUAL = 0x7A,
        VAR_EQUAL = 0x7B,
        VAR_NOT_EQUAL = 0x7C,
        ROUND = 0x7D,
        ROUND_CEIL = 0x7E,
        ROUND_FLOOR = 0x7F,
        I_DONT_KNOW1 = 0x80,
        I_DONT_KNOW2 = 0x81,
        I_DONT_KNOW3 = 0x82,
        OFFSET_TOP = 0x83,
        OFFSET_LEFT = 0x84,
        OFFSET_BOTTOM = 0x85,
        OFFSET_RIGHT = 0x86,
        CHECK_FORWARD = 0x87,
        OFFSET_FORWARD = 0x88,
        CALL_FUNC_STATE = 0x89,
        RETURN_STATE = 0x8A,
        YES_NO = 0x8B,
        NO_YES = 0x8C,
        STD_DIG = 0x8D,
        STD_BUILD = 0x8E,
        STD_HEAL = 0x8F,
        FLIP = 0x90,
        STD_DIG_AROUND = 0x91,
        IS_IN_GUN = 0x92,
        AMMO = 0x93,
        IS_HP_LESS_100 = 0x94,
        IS_HP_LESS_50_2 = 0x95,
        YES_NO_NEWLINE = 0x96,
        NO_YES_NEWLINE = 0x97,
        YES_NO_START = 0x98,
        NO_YES_START = 0x99,
        YES_NO_STOP = 0x9A,
        NO_YES_STOP = 0x9B,
        CHECK_LEFT_HAND = 0x9C,
        CHECK_RIGHT_HAND = 0x9D,
        AUTO_DIG_ON = 0x9E,
        AUTO_DIG_OFF = 0x9F,
        AGR_ON = 0xA0,
        AGR_OFF = 0xA1,
        USE_BOOM = 0xA2,
        USE_RAZRYAD = 0xA3,
        USE_PROTON = 0xA4,
        BUILD_WB = 0xA5,
        RESPAWN_TO = 0xA6,
        USE_GEOPACK = 0xA7,
        USE_ZZ = 0xA8,
        USE_C190 = 0xA9,
        USE_POLIMER = 0xAA,
        UP = 0xAB,
        CRAFT = 0xAC,
        USE_NANOBOT = 0xAD,
        USE_REMBOT = 0xAE,
        DIR_INV_TOP = 0xAF,
        DIR_INV_LEFT = 0xB0,
        DIR_INV_BOTTOM = 0xB1,
        DIR_INV_RIGHT = 0xB2,
        MODE_MANUAL = 0xB3,
        MODE_AUTO = 0xB4,
        I_DONT_KNOW4 = 0xB5,
        I_DONT_KNOW5 = 0xB6
    }

    public static class CommandExtensions
    {
        // Группировки по вводу значений
        public static readonly HashSet<Command> NO_ARGS = new HashSet<Command>();
        public static readonly HashSet<Command> ONE_ARGS = new HashSet<Command>();
        public static readonly HashSet<Command> TWO_ARGS = new HashSet<Command>();
        
        // Группировки общей тематики
        public static readonly HashSet<Command> ACTION = new HashSet<Command>();
        
        static CommandExtensions()
        {
            // Определяем диапазоны для каждой группы
            var noArgsRanges = new List<(int start, int end)>
            {
                (0x00, 0x17), (0x1B, 0x27), (0x29, 0x60), (0x76, 0x76),
                (0x83, 0x88), (0x8A, 0x8A), (0x8D, 0xA5), (0xA7, 0xB4)
            };
            
            var oneArgRanges = new List<(int start, int end)>
            {
                (0x18, 0x1A), (0x28, 0x28), (0x61, 0x62), (0x68, 0x6B),
                (0x70, 0x75), (0x7D, 0x7F), (0x89, 0x89), (0x8B, 0x8C),
                (0xA6, 0xA6), (0xB5, 0xB6)
            };
            
            var twoArgsRanges = new List<(int start, int end)>
            {
                (0x63, 0x67), (0x6C, 0x6F), (0x77, 0x7C), (0x80, 0x82)
            };
            
            // Заполняем группы
            NO_ARGS.UnionWith(GetCommandsInRanges(noArgsRanges));
            ONE_ARGS.UnionWith(GetCommandsInRanges(oneArgRanges));
            TWO_ARGS.UnionWith(GetCommandsInRanges(twoArgsRanges));
            
            // Заполняем ACTION группу
            ACTION.UnionWith(new[]
            {
                Command.MOVE_BOTTOM, Command.MOVE_LEFT, Command.MOVE_RIGHT, Command.MOVE_TOP,
                Command.REPEAT, Command.MOVE_FORWARD, Command.DIR_BOTTOM, Command.DIR_TOP,
                Command.DIR_LEFT, Command.DIR_RIGHT, Command.DIR_INV_BOTTOM, Command.DIR_INV_LEFT,
                Command.DIR_INV_RIGHT, Command.DIR_INV_TOP, Command.DIR_RANDOM, Command.ROTATE_LEFT,
                Command.ROTATE_RIGHT, Command.BUILD_BLOCK, Command.BUILD_QUADRO, Command.BUILD_ROAD,
                Command.BUILD_WB, Command.GEO, Command.HEAL, Command.STD_BUILD, Command.STD_DIG,
                Command.STD_DIG_AROUND, Command.STD_HEAL, Command.BOX_ALL, Command.BOX_BLUE,
                Command.BOX_WHITE, Command.BOX_CYAN, Command.BOX_GREEN, Command.BOX_HALF,
                Command.BOX_RED, Command.BOX_VIOLET, Command.USE_BOOM, Command.USE_C190,
                Command.USE_GEOPACK, Command.USE_NANOBOT, Command.USE_POLIMER, Command.USE_PROTON,
                Command.USE_RAZRYAD, Command.USE_REMBOT, Command.USE_ZZ
            });
        }
        
        private static IEnumerable<Command> GetCommandsInRanges(List<(int start, int end)> ranges)
        {
            foreach (Command cmd in Enum.GetValues(typeof(Command)))
            {
                int cmdInt = (int)cmd;
                foreach (var (start, end) in ranges)
                {
                    if (start <= cmdInt && cmdInt <= end)
                    {
                        yield return cmd;
                        break;
                    }
                }
            }
        }
        
        public static string ToLowerString(this Command cmd)
        {
            return cmd.ToString().ToLower();
        }

        // С получением всех значений
        public static IEnumerable<Command> GetAllCommands()
        {
            return Enum.GetValues(typeof(Command)).Cast<Command>();
        }

        // Получение команды по индексу в enum (порядковый номер, не значение)
        public static Command GetByIndex(int index)
        {
            var commands = GetAllCommands().ToList();
            if (index >= 0 && index < commands.Count)
            {
                return commands[index];
            }
            throw new ArgumentOutOfRangeException(nameof(index), "Индекс вне диапазона");
        }
    }

}


// // Пример использования:
// public class Program
// {
//     public static void Main()
//     {
//         // Проверка групп
//         Console.WriteLine("NO_ARGS count: " + CommandExtensions.NO_ARGS.Count);
//         Console.WriteLine("ONE_ARGS count: " + CommandExtensions.ONE_ARGS.Count);
//         Console.WriteLine("TWO_ARGS count: " + CommandExtensions.TWO_ARGS.Count);
//         Console.WriteLine("ACTION count: " + CommandExtensions.ACTION.Count);
        
//         // Проверка ToLowerString
//         Console.WriteLine(Command.MOVE_TOP.ToLowerString()); // "move_top"
//         Console.WriteLine(Command.BUILD_BLOCK.ToLowerString()); // "build_block"
        
//         // Проверка принадлежности
//         Console.WriteLine(CommandExtensions.ACTION.Contains(Command.MOVE_TOP)); // True
//         Console.WriteLine(CommandExtensions.NO_ARGS.Contains(Command.EMPTY)); // True
//         Console.WriteLine(CommandExtensions.ONE_ARGS.Contains(Command.GO_TO)); // True
//     }
// }