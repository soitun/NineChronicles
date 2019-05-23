using System;
using System.Collections.Generic;

namespace Nekoyume.Action
{
    [Serializable]
    public struct GameActionErrorCode
    {
        public const int Success = 0;
        public const int Fail = -1;
        public const int UnexpectedInternalAction = -2;
        public const int KeyNotFoundInTable = -3;

        #region Sell

        public const int SellItemNotFoundInInventory = -100;
        public const int SellItemCountNotEnoughInInventory = -101;

        #endregion

        #region Buy

        public const int BuyGoldNotEnough = -100;
        public const int BuySoldOut = -101;

        #endregion
    }
}
