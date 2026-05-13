using Xunit;

namespace LanguageTests;

public class Demo_Records
{
	[Fact]
	public void Record_Classic()
	{
		var product = new Product()
		{
			Id = 1,
			Name = "test",
		};

		// product.Name = "error"; // Error
		product.NameWritable = "ok";
	}

	// public record class Product
	public record Product
	{
		public required int Id { get; init; }
		public required string Name { get; init; }
		public string NameWritable { get; set; }
	}


	[Fact]
	public void Record_Concise_PositionalRecord()
	{
		var item = new Item(10, "n");
		// item.Id = 11; // Error
	}
	public record Item(int Id, string Name);

	[Fact]
	public void Record_Immutability()
	{
		var item = new Item(10, "n");

		var item2 = item with
		{
			Name = "n2"
		};
	}

	[Fact]
	public void Record_Comparison()
	{
		var item = new Item(10, "a");
		var item2 = item with { Name = "b" };
		var item3 = new Item(10, "a");

		Assert.NotEqual(item, item2);
		Assert.Equal(item, item3);
	}

	public record Vehicle(int Id, string Name);

	// Positional
	public record Car(int Id, string Name, string Color) : Vehicle(Id, Name);

	// Classical
	public record Motocycle : Vehicle
	{
		public string Color { get; init; }

		public Motocycle(int id, string name, string color) : base(id, name)
		{
			Color = color;
		}
	}

	[Fact]
	public void Record_Inheritance()
	{
		var car = new Car(1, "c", "r");
		var moto = new Motocycle(2, "m", "m");

		Assert.Equal("Car { Id = 1, Name = c, Color = r }", car.ToString());
		Assert.Equal("Motocycle { Id = 2, Name = m, Color = m }", moto.ToString());
	}

	public readonly record struct Point(int x, int y);
	public record struct PointStruct(int x, int y);
	public record PointRec(int x, int y);

	[Fact]
	public void Record_Struct()
	{
		var p1 = new Point(1, 2);
		var p2 = new PointStruct(1, 2);
		Assert.False(p1.Equals(p2));

		// Error: p1.x = 1; 
		p2.x = 1;
	}

	[Fact]
	public void RecordStructVsRecord()
	{
		// Equality Test
		var ps1 = new PointStruct(1, 2);
		var ps2 = new PointStruct(1, 2);
		Assert.True(ps1.Equals(ps2));
		Assert.False(ReferenceEquals(ps1, ps2));

		var ps3 = ps1;
		Assert.False(ReferenceEquals(ps1, ps3));

		var pr1 = new PointRec(1, 2);
		var pr2 = new PointRec(1, 2);
		Assert.True(pr1.Equals(pr2));
		Assert.False(ReferenceEquals(pr1, pr2));

		var pr4 = pr1;
		Assert.True(ReferenceEquals(pr1, pr4));
	}
}