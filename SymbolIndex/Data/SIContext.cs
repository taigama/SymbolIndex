namespace SymbolIndex.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Linq;
    using Models;

    public class SIContext : DbContext
    {
        public SIContext()
            : base("name=SIContextGoDaddy")
        {
        }

        public DbSet<Font> Fonts { get; set; }
        public DbSet<Symbol> Symbols { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<FeedbackModel> Feeds { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();
        }
        
        public void Seed(SIContext context)
        {
#if DEBUG


#endif
            var fonts = new List<Font>
            {
                new Font
                {
                    FontName = "default Unicode",
                    FontUrl = ""
                }
            };
            fonts.ForEach(x => context.Fonts.Add(x));


            var tags = new List<Tag>
            {
                new Tag
                {
                    TagString = "just test"
                }
            };
            tags.ForEach(x => context.Tags.Add(x));

            var symbols = new List<Symbol>
            {
                new Symbol
                {
                    Font = fonts[0],
                    Content = "a"
                }
            };
            symbols.ForEach(x => context.Symbols.Add(x));

            context.SaveChanges();

            var sym = context.Symbols.FirstOrDefault();
            sym.Tags = new List<Tag>
            {
                context.Tags.FirstOrDefault()
            };

            context.Entry(sym).State = EntityState.Modified;
            context.SaveChanges();
        }

        public class DropCreateIfChangeInitializer : DropCreateDatabaseIfModelChanges<SIContext>
        {
            protected override void Seed(SIContext context)
            {
                context.Seed(context);

                base.Seed(context);
            }
        }

        public class CreateInitializer : CreateDatabaseIfNotExists<SIContext>
        {
            protected override void Seed(SIContext context)
            {
                context.Seed(context);

                base.Seed(context);
            }
        }

        static SIContext()
        {
#if DEBUG
            Database.SetInitializer<SIContext>(new DropCreateIfChangeInitializer());
#else
            Database.SetInitializer<SIContext> (new CreateInitializer ());
#endif
        }
    }

    //public class MyEntity
    //{
    //    public int Id { get; set; }
    //    public string Name { get; set; }
    //}
}