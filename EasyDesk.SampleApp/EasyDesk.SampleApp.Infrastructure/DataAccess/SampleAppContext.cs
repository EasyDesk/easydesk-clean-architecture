﻿using EasyDesk.CleanArchitecture.Dal.EfCore.Entities;
using EasyDesk.SampleApp.Infrastructure.DataAccess.Model;
using Microsoft.EntityFrameworkCore;

namespace EasyDesk.SampleApp.Infrastructure.DataAccess
{
    public class SampleAppContext : EntitiesContext
    {
        public DbSet<PersonModel> People { get; set; }

        public SampleAppContext(DbContextOptions<SampleAppContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SampleAppContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}