using System;
using System.Collections.Generic;

namespace Nekoyume.L10n
{
    /// <summary>
    /// UnityEngine.SystemLanguage 안에서 선택적으로 포함합니다.
    /// </summary>
    [Serializable]
    public enum LanguageType
    {
        /// <summary>
        /// font file: Assets/Resources/Font/TTF/PoorStory-Regular.ttf
        /// font asset file: Assets/Resources/Font/SDF/PoorStory-Regular SDF.asset
        ///     - Sampling Font Size: Auto Sizing
        ///     - Padding: 9
        ///     - Packing Method: Fast
        ///     - Atlas Resolution: 4096x4096
        ///     - Character Set: Characters From File
        ///     - Character File: Assets/Resources/Font/CharacterFiles/KS1001.txt
        ///     - Render Mode: SDFAA
        ///     - Get Kerning Pairs: true
        /// </summary>
        English,

        /// <summary>
        /// This is same with English.
        /// </summary>
        Korean,

        /// <summary>
        /// font file: Assets/Resources/Font/TTF/PoorStory-Latin.otf
        /// font asset file: Assets/Resources/Font/SDF/PoorStory-Latin SDF.asset
        ///     - Sampling Font Size: Auto Sizing
        ///     - Padding: 9
        ///     - Packing Method: Fast
        ///     - Atlas Resolution: 516x516
        ///     - Character Set: Unicode Range (Hex)
        ///     - Character Sequence (Hex): 00C0-00D6,00D8-00F6,00F8-00FF
        ///           https://en.wikipedia.org/wiki/Latin-1_Supplement_(Unicode_block)
        ///     - Render Mode: SDFAA
        ///     - Get Kerning Pairs: true
        /// </summary>
        PortugueseBrazil,

        /// <summary>
        /// This is same with Portuguese.
        /// </summary>
        Polish,

        /// <summary>
        /// font file: Assets/Resources/Font/TTF/NotoSansCJKjp-Regular.otf
        /// font asset file 1: Assets/Resources/Font/SDF/Hanazome-00-ASCII(99) SDF.asset
        ///     - Sampling Font Size: Auto Sizing
        ///     - Padding: 7
        ///     - Packing Method: Fast
        ///     - Atlas Resolution: 516x516
        ///     - Character Set: ASCII
        ///     - Render Mode: SDFAA
        ///     - Get Kerning Pairs: true
        /// font asset file 2: Assets/Resources/Font/SDF/Hanazome-01-2538(2339) SDF SDF.asset
        ///     - Sampling Font Size: Auto Sizing
        ///     - Padding: 7
        ///     - Packing Method: Fast
        ///     - Atlas Resolution: 4096x4096
        ///     - Character Set: Custom Range
        ///     - Character Sequence (Decimal): Assets/Resources/Font/CharacterFiles/japanese-custom-range-01-2538.txt
        ///           https://www.youtube.com/watch?v=Dj4XaZJTEQM
        ///           https://gist.github.com/boscohyun/9ca2fc65b0e042bab999c9adce4d4094
        ///     - Render Mode: SDFAA
        ///     - Get Kerning Pairs: true
        /// </summary>
        Japanese,

        /// <summary>
        /// font file: Assets/Resources/Font/TTF/NotoSansCJKsc-Regular.otf
        /// font asset file: Assets/Resources/Font/SDF/NotoSansCJKsc-Regular-00-ASCII(98) SDF.asset
        ///     - Sampling Font Size: Auto Sizing
        ///     - Padding: 7
        ///     - Packing Method: Fast
        ///     - Atlas Resolution: 512x512
        ///     - Character Set: Unicode Range (Hex)
        ///     - Character Sequence (Hex): 20-7E,A0,2026,25A1,3001,3002,FF08,FF09,FF0C,FF1F
        ///           ASCII + @
        ///     - Render Mode: SDFAA
        ///     - Get Kerning Pairs: true
        /// font asset files: Assets/Resources/Font/SDF/NotoSansCJKsc-Regular-{00}-{0000}({0000}) SDF.asset
        ///     - Sampling Font Size: Auto Sizing
        ///     - Padding: 7
        ///     - Packing Method: Fast
        ///     - Atlas Resolution: 4096x4096
        ///     - Character Set: Unicode Range (Hex)
        ///     - Character Sequence (Hex): Assets/Resources/Font/CharacterFiles/simplified-chinese-8105-unicode-range-{00}-{0000}.txt
        ///           http://hanzidb.org/character-list/general-standard
        ///     - Render Mode: SDFAA
        ///     - Get Kerning Pairs: true
        /// </summary>
        ChineseSimplified,

        /// <summary>
        /// font file: Assets/Resources/Font/TTF/kanit-regular.otf
        /// font asset file: Assets/Resources/Font/SDF/kanit-regular-00-ASCII(97) SDF.asset
        ///     - Sampling Font Size: Auto Sizing
        ///     - Padding: 7
        ///     - Packing Method: Fast
        ///     - Atlas Resolution: 516x516
        ///     - Character Set: ASCII
        ///     - Render Mode: SDFAA
        ///     - Get Kerning Pairs: true
        /// font asset file: Assets/Resources/Font/SDF/kanit-regular-01-87(87) SDF.asset
        ///     - Sampling Font Size: Auto Sizing
        ///     - Padding: 7
        ///     - Packing Method: Fast
        ///     - Atlas Resolution: 516x516
        ///     - Character Set: Unicode Range (Hex)
        ///     - Character Sequence (Hex): 0E01-0E3A,0E3F-0E5B
        ///           https://en.wikipedia.org/wiki/Thai_(Unicode_block)
        ///     - Render Mode: SDFAA
        ///     - Get Kerning Pairs: true
        /// </summary>
        Thai,
    }

    public class LanguageTypeComparer : IEqualityComparer<LanguageType>
    {
        public static readonly LanguageTypeComparer Instance = new LanguageTypeComparer();

        public bool Equals(LanguageType x, LanguageType y)
        {
            return x == y;
        }

        public int GetHashCode(LanguageType obj)
        {
            return obj.GetHashCode();
        }
    }
}
