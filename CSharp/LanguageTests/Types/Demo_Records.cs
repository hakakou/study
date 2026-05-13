using Xunit;
using LanguageTests;

namespace LanguageTests.Types;

public class Demo_Records
{
	[Fact]
	public void Record_Classic()
	{
		var product = new Types.Demo_Records.Product()
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
		var item = new Types.Demo_Records.Item(10, "n");
		// item.Id = 11; // Error
	}
	public record Item(int Id, string Name);

	[Fact]
	public void Record_Immutability()
	{
		var item = new Types.Demo_Records.Item(10, "n");

		var item2 = item with
		{
			Name = "n2"
		};
	}

	[Fact]
	public void Record_Comparison()
	{
		var item = new Types.Demo_Records.Item(10, "a");
		var item2 = item with { Name = "b" };
		var item3 = new Types.Demo_Records.Item(10, "a");

		Assert.NotEqual(item, item2);
		Assert.Equal(item, item3);
	}

	public record Vehicle(int Id, string Name);

	// Positional
	public record Car(int Id, string Name, string Color) : Types.Demo_Records.Vehicle(Id, Name);

	// Classical
	public record Motocycle : Types.Demo_Records.Vehicle
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
		var car = new Types.Demo_Records.Car(1, "c", "r");
		var moto = new Types.Demo_Records.Motocycle(2, "m", "m");

		Assert.Equal("Car { Id = 1, Name = c, Color = r }", car.ToString());
		Assert.Equal("Motocycle { Id = 2, Name = m, Color = m }", moto.ToString());
	}

	public readonly record struct Point(int x, int y);
	public record struct PointStruct(int x, int y);
	public record PointRec(int x, int y);

	[Fact]
	public void Record_Struct()
	{
		var p1 = new Types.Demo_Records.Point(1, 2);
		var p2 = new Types.Demo_Records.PointStruct(1, 2);
		Assert.False(p1.Equals(p2));

		// Error: p1.x = 1; 
		p2.x = 1;
	}

	[Fact]
	public void RecordStructVsRecord()
	{
		// Equality Test
		var ps1 = new Types.Demo_Records.PointStruct(1, 2);
		var ps2 = new Types.Demo_Records.PointStruct(1, 2);
		Assert.True(ps1.Equals(ps2));
		Assert.False(ReferenceEquals(ps1, ps2));

		var ps3 = ps1;
		Assert.False(ReferenceEquals(ps1, ps3));

		var pr1 = new Types.Demo_Records.PointRec(1, 2);
		var pr2 = new Types.Demo_Records.PointRec(1, 2);
		Assert.True(pr1.Equals(pr2));
		Assert.False(ReferenceEquals(pr1, pr2));

		var pr4 = pr1;
		Assert.True(ReferenceEquals(pr1, pr4));
	}
}