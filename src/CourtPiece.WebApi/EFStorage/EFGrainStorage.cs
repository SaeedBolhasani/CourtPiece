using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Orleans.Configuration;
using Orleans.Runtime;
using Orleans.Storage;

public sealed class EFGrainStorage : IGrainStorage, ILifecycleParticipant<ISiloLifecycle>
{
    private readonly string _storageName;
    private readonly Func<ApplicationDbContext> _appliactionDbContextFactory;
    private string GetKeyString(string grainType, GrainId grainId) => grainId.ToString();
    //$"{_clusterOptions.ServiceId}.{grainId.Key}.{grainType}";
    public EFGrainStorage(
        string storageName,
        Func<ApplicationDbContext> appliactionDbContextFactory)
    {
        _storageName = storageName;
        _appliactionDbContextFactory = appliactionDbContextFactory;
   }

    public async Task ClearStateAsync<T>(
    string stateName,
    GrainId grainId,
    IGrainState<T> grainState)
    {
        var fName = GetKeyString(stateName, grainId);
        using var context = _appliactionDbContextFactory();

        var state = await context.GrainStates.FindAsync(fName);
        context.GrainStates.Remove(state);
        await context.SaveChangesAsync();

    }

    public async Task ReadStateAsync<T>(
    string stateName,
    GrainId grainId,
    IGrainState<T> grainState)
    {
        if (!grainState.RecordExists) return;

        var fName = GetKeyString(stateName, grainId);
        using var context = _appliactionDbContextFactory();

        var state = await context.GrainStates.FindAsync(fName);

        grainState.State = JsonConvert.DeserializeObject<T>(state.JsonValue);
        grainState.ETag = state.CreateDateTime.ToString();
    }

    public async Task WriteStateAsync<T>(
      string stateName,
      GrainId grainId,
      IGrainState<T> grainState)
    {
        var fName = GetKeyString(stateName, grainId);

        using var context = _appliactionDbContextFactory();

        var state = await context.GrainStates.FindAsync(fName);
        if (state == null)
        {
            state = new GrainState
            {
                Id = fName,
                CreateDateTime = DateTime.Now,
                JsonValue = JsonConvert.SerializeObject(grainState.State)
            };
            await context.GrainStates.AddAsync(state);
        }

        state.JsonValue = JsonConvert.SerializeObject(grainState.State);
        await context.SaveChangesAsync();

    }

    public void Participate(ISiloLifecycle lifecycle) =>
     lifecycle.Subscribe(
         observerName: OptionFormattingUtilities.Name<EFGrainStorage>(_storageName),
         stage: ServiceLifecycleStage.ApplicationServices,
         onStart: (ct) =>
         {
             //Directory.CreateDirectory(_options.RootDirectory);
             return Task.CompletedTask;
         });
}
