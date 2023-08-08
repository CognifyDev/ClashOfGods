using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static COG.UI.CustomOption.CustomOption;
using static COG.Config.Impl.LanguageConfig;


namespace COG.UI.CustomOption;

    public class GameOption
    {
        public static CustomOption? NeutralNumber;
        public static CustomOption? NeverEndGame;
        public static void GameOptions()
        {

            NeutralNumber = Create(0, CustomOptionType.Neutral, Instance.NeutralNumber, 0,0,15,1,null);
            NeverEndGame = Create(1, CustomOptionType.General, Instance.NeverGameEnd, false, null);
        }
    }


