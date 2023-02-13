﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mmo_shared.Messages {
    public class CharSelectInfo : Message {
        [Order(0)] public CharSlotInfo[] Chars { get; set; }

        public CharSelectInfo() { }

        public CharSelectInfo(CharSlotInfo[] chars) {
            Chars = chars;
        }
    }
}
