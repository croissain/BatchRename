using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RenameRuleLib
{
    public interface IRenameRuleParser
    {
        string MagicWord { get; }
        IRenameRule Parse(string line);
    }
}
