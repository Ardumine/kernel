using Kernel.AFCP;
using Kernel.Modules.Base;

internal class ExistingDataChannelDescriptor : IExistingDataChannelDescriptor
{
    public required string Path { get; set; }
    public required Guid OwningKernelGUID { get; set; }

    internal static IExistingDataChannelDescriptor? FromAFCP(DataChannelDescriptor? afcpChannel)
    {
        if (afcpChannel == null)
            return null;
        return new ExistingDataChannelDescriptor()
        {
            Path = afcpChannel.Path,
            OwningKernelGUID = afcpChannel.OwningKernel
        };
    }
 
}