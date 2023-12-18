namespace FrontendRegulatorAccountEnrollment.Core.Exceptions;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

[Serializable]
[ExcludeFromCodeCoverage]
public class NoOrganisationAllowListDataException : Exception
{
    public NoOrganisationAllowListDataException()
    {
    }

    public NoOrganisationAllowListDataException(string message)
        : base(message)
    {
    }

    public NoOrganisationAllowListDataException(string message, Exception inner)
        : base(message, inner)
    {
    }

    protected NoOrganisationAllowListDataException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

}