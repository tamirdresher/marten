﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Marten.Schema;
using Marten.Testing.Documents;

namespace Marten.Testing
{

    public class TestingDocumentStore : DocumentStore
    {
        public static int SchemaCount = 0;
        private static readonly object _locker = new object();

        public new static DocumentStore For(Action<StoreOptions> configure)
        {
            var options = new StoreOptions();
            options.Connection(ConnectionSource.ConnectionString);
            options.Serializer<TestsSerializer>();

            options.NameDataLength = 100;

            configure(options);

            

            var store = new TestingDocumentStore(options);
            store.Advanced.Clean.CompletelyRemoveAll();

            return store;
        }


        public static DocumentStore Basic()
        {
            return For(_ =>
            {
            }).As<DocumentStore>();
        }

        public static DocumentStore DefaultSchema()
        {
            var store = For(_ =>
            {
                _.DatabaseSchemaName = StoreOptions.DefaultDatabaseSchemaName;
            });
            return store;
        }

        private TestingDocumentStore(StoreOptions options) : base(options)
        {
        }

        public override void Dispose()
        {
            var schemaName = Options.DatabaseSchemaName;

            if (schemaName != StoreOptions.DefaultDatabaseSchemaName)
            {
                var sql = $"DROP SCHEMA {schemaName} CASCADE;";
                using (var conn = Tenancy.Default.OpenConnection())
                {
                    conn.Execute(cmd => cmd.CommandText = sql);
                }
            }

            
            base.Dispose();


        }
    }
}