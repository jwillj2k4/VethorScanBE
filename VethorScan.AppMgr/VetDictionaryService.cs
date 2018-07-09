using System;
using System.Collections.Generic;
using VethorScan.Contracts;

namespace VethorScan.AppMgr
{
    public class VetDictionaryService
    {
        public readonly Dictionary<Func<decimal, bool>, NodeType> NodeDictionary =
            new Dictionary<Func<decimal, bool>, NodeType>
            {
                {x => x < 600000, NodeType.None},

                {x => x < 1000000, NodeType.VeThorX},

                {x => x < 1600000, NodeType.Strength | NodeType.VeThorX},

                {x => x < 5000000, NodeType.Strength | NodeType.StrengthX},

                {x => x < 5600000, NodeType.Thunder | NodeType.StrengthX},

                {x => x < 15000000, NodeType.Thunder | NodeType.ThunderX},

                {x => x < 15600000, NodeType.Mjolnir | NodeType.ThunderX},

                {x => x < 25000000, NodeType.Mjolnir | NodeType.MjolnirX},

                {x => x >= 25000000, NodeType.Mjolnir | NodeType.MjolnirX | NodeType.Thrudheim},
            };
    }
}