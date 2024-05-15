using DiagnoseDataObjects;

namespace DiagnoseCardiovascularService.Server
{
    internal interface IDiagnoseServer
    {
        Task DiagnoseAsync(PrepairedWetData data);
    }
}