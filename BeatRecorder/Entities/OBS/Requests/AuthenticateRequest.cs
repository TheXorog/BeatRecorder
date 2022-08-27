using Newtonsoft.Json.Linq;
using System.Text.Json.Nodes;

namespace BeatRecorder.Entities.OBS;

internal class Indentify : BaseRequest
{
    internal Indentify(string auth = "", int rpcVersion = 1)
    {
        this.op = 1;
        this.d = new JObject
        {
            ["rpcVersion"] = rpcVersion,
        };

        if (!auth.IsNullOrWhiteSpace())
            ((JObject)this.d).Add("authentication", auth);
    }
}
