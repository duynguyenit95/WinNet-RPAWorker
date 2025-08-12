using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RPA.Core;

namespace RPA.Core.Data
{
    public class EntityConfigure<T> : IEntityTypeConfiguration<T>
        where T : Entity
    {
        public virtual void Configure(EntityTypeBuilder<T> builder)
        {
            // Primary Key
            builder.HasKey(t => t.ID);
            // Properties
            builder.Property(t => t.LastUpdatedBy).HasMaxLength(50);
            builder.Property(t => t.LastUpdatedTime).HasColumnType("datetime");
            builder.HasIndex(t => t.IsActive).IsUnique(false);
        }
    }

    public class RegexInforMap : EntityConfigure<RegexInfor>
    {
        public override void Configure(EntityTypeBuilder<RegexInfor> builder)
        {
            base.Configure(builder);
            // Properties
            builder.Property(t => t.CustomerID).IsRequired();
            builder.Property(t => t.Name).IsRequired().HasMaxLength(256);
            builder.Property(t => t.Pattern).IsRequired().HasMaxLength(256);
            builder.Property(t => t.DateFormat).IsRequired().HasMaxLength(20);
        }
    }

    public class SupplierMap : EntityConfigure<Supplier>
    {
        public override void Configure(EntityTypeBuilder<Supplier> builder)
        {
            base.Configure(builder);
            // Properties
            builder.Property(t => t.Name).IsRequired().HasMaxLength(256);
            builder.Property(t => t.InvoiceFormRecognizerModel).HasMaxLength(48);
            builder.Property(t => t.PIOCFormRecognizerModel).HasMaxLength(48);
            builder.Property(t => t.SAPID).HasMaxLength(10);
        }
    }

    public class FormRecognizerMap : EntityConfigure<FormRecognizerLog>
    {
        public override void Configure(EntityTypeBuilder<FormRecognizerLog> builder)
        {
            base.Configure(builder);
            // Properties
            builder.Property(t => t.FileName).IsRequired().HasMaxLength(256);
            builder.Property(t => t.ModelID).IsRequired().HasMaxLength(48);
            builder.Property(t => t.Type).IsRequired().HasMaxLength(32);
            builder.Property(t => t.SupplierID).IsRequired();
            //builder.HasIndex(t => new { t.SupplierID, t.ModelID, t.FileName, t.Type }).IsUnique(true);

        }
    }

    public class WorkerInforMap : EntityConfigure<WorkerInfor>
    {
        public override void Configure(EntityTypeBuilder<WorkerInfor> builder)
        {
            base.Configure(builder);
            builder.Ignore(x => x.FileName);
#if DEBUG
            builder.ToTable("WorkerInfors2");
#endif
            // Properties
            builder.Property(t => t.Name).IsRequired().HasMaxLength(128);
            builder.Property(t => t.Version).IsRequired().HasMaxLength(32);
            builder.Property(t => t.DownloadPath).IsRequired();
            builder.HasIndex(t => t.Name).IsUnique(true);

        }
    }

    public class WorkerConfigurationMap : EntityConfigure<WorkerConfiguration>
    {
        public override void Configure(EntityTypeBuilder<WorkerConfiguration> builder)
        {
            base.Configure(builder);
            // Properties
            builder.Property(t => t.Name).IsRequired().HasMaxLength(128);
            builder.Property(t => t.Group).IsRequired().HasMaxLength(128);
        }
    }
    public class QueueTaskMap : EntityConfigure<QueueTask>
    {
        public override void Configure(EntityTypeBuilder<QueueTask> builder)
        {
            base.Configure(builder);
            // Properties
            builder.Property(t => t.ActionName).IsRequired().HasMaxLength(128);
            builder.Property(t => t.RequestGroup).IsRequired().HasMaxLength(128);
            builder.Property(t => t.RequestWorker).IsRequired().HasMaxLength(128);
            builder.Property(t => t.ProcessGroup).IsRequired().HasMaxLength(128);
            builder.Property(t => t.ProcessWorker).IsRequired().HasMaxLength(128);
        }
    }
}
