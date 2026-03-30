using Mews.Infrastructure.Resources;
using Mews.Infrastructure.Sdk.ResourceDeployment;
using Pulumi.AzureNative.Sql;

namespace Mews.Job.Scheduler.Infrastructure;

public sealed class SqlDatabaseConfiguration
{
    public SqlDatabaseTier Tier { get; }
    public bool ZoneRedundancy { get; }
    public int HighAvailabilityReplicaCount { get; }
    public BackupStorageRedundancy BackupStorageRedundancy { get; }
    public bool GeoReplicaInPairedRegion { get; }

    public SqlDatabaseConfiguration(InfrastructureBuilder builder)
    {
        var cpuCount = builder.GetInt("sql-server-cpu-count");
        var zoneRedundancy = builder.GetBool("sql-server-zone-redundancy");
        var highAvailabilityReplicaCount = builder.GetInt("sql-server-high-availability-replica-count");
        var backupStorageRedundancy = builder.GetString("sql-server-backup-storage-redundancy").Trim().ToLower();
        var geoReplicaInPairedRegion = builder.GetBool("sql-server-geo-replica-in-paired-region");

        Tier = new Hyperscale(CpuCount: cpuCount);
        ZoneRedundancy = zoneRedundancy;
        HighAvailabilityReplicaCount = highAvailabilityReplicaCount;
        BackupStorageRedundancy = backupStorageRedundancy switch
        {
            "geo" => BackupStorageRedundancy.Geo,
            "local" => BackupStorageRedundancy.Local,
            "zone" => BackupStorageRedundancy.Zone,
            "geozone" => BackupStorageRedundancy.GeoZone,
            _ => throw new Exception($"Unsupported backup storage redundancy: {backupStorageRedundancy}")
        };
        GeoReplicaInPairedRegion = geoReplicaInPairedRegion;
    }
}
