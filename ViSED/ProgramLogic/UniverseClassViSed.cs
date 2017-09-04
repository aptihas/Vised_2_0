using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ViSED.ProgramLogic
{
    public static class UniverseClassViSed
    {
        public static string Words(string _str, int _num)
        {
            if(_str.Split(' ').Length > _num)
            {
                string str=null;
                for(int i = 0; i < _num; i++)
                {
                    str += _str.Split(' ')[i]+" ";
                }
                return str.Trim()+" ...";
            }
            else
            {
                return _str;
            }
        }
    }
}