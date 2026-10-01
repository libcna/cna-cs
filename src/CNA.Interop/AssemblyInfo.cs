using System.Runtime.CompilerServices;

// CNA.Interop is intentionally all-internal (see docs/architecture.md): it is the only
// project allowed to talk to native code directly. CNA.Framework is the sole consumer.
[assembly: InternalsVisibleTo("CNA.Framework")]
// GamerServices, Avatar and Net exist only to satisfy XNA and have no CNA.* layer of their own,
// so the facade calls their C ABI directly; see docs/architecture.md.
[assembly: InternalsVisibleTo("CNA.XnaCompat")]
// The opt-in phone assembly is the same kind of surface: Microsoft.Devices over devices.h/sensors.h.
[assembly: InternalsVisibleTo("CNA.PhoneCompat")]
[assembly: InternalsVisibleTo("CNA.Interop.Tests")]
[assembly: InternalsVisibleTo("CNA.Framework.Tests")]
[assembly: InternalsVisibleTo("CNA.GamerServices.IntegrationTests")]
[assembly: InternalsVisibleTo("CNA.AbiVerify")]
