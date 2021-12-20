using System;

namespace RenameRuleLib
{
    public interface IRenameRule
    {
        string MagicWord { get; }
        string Rename(string original, int index);
        string Config(IRenameRule ruleItem);
        IRenameRule Clone();
    }

}
