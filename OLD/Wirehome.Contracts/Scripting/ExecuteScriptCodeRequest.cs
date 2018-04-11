namespace Wirehome.Contracts.Scripting
{
    public class ExecuteScriptCodeRequest
    {
        public string ScriptCode { get; set; }

        public string EntryMethodName { get; set; }
    }
}
