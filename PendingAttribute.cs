﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cone
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class PendingAttribute : Attribute 
    {
        public string Reason;
    }
}
