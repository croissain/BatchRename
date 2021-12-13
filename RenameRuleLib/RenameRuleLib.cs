using System;

namespace RenameRuleLib
{
    public interface IRenameRule
    {
        string MagicWord { get; }
        string Rename(string original);
        string Config(IRenameRule ruleItem);
        IRenameRule Clone();
    }

}
