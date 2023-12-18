using System.Diagnostics.CodeAnalysis;

namespace FrontendRegulatorAccountEnrollment.Core.Constants
{
    [ExcludeFromCodeCoverage]
    public class Nation
    {
        private readonly Dictionary<int, string> _keyValuePairs;

        public Nation()
        {
            _keyValuePairs = new Dictionary<int, string>{
                {1, "England"},
                {2, "Northern Ireland"},
                {3, "Scotland"},
                {4, "Wales" }
            };
        }

        public string GetNationName(int nationId)
        {
            return _keyValuePairs[nationId];
        }
    }
}
