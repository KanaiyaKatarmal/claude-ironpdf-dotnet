using OrderPdf.Domain.Entities;
using OrderPdf.Domain.Enums;

namespace OrderPdf.Infrastructure.Data;

public static class SampleDataSeeder
{
    public static Company GetDefaultCompany()
    {
        return new Company
        {
            Id = "COMP-001",
            Name = "Acme Commerce Solutions Inc.",
            Tagline = "Next-Generation Enterprise Cloud & Logistics",
            Email = "orders@acmecommerce.com",
            Phone = "+1 (800) 555-0199",
            Website = "https://acmecommerce.com",
            TaxId = "US-TAX-84920194",
            Address = new Address
            {
                Street = "100 Technology Plaza",
                SuiteOrApt = "Suite 500",
                City = "Seattle",
                State = "WA",
                PostalCode = "98101",
                Country = "United States"
            }
        };
    }

    public static List<Order> GetSampleOrders()
    {
        var company = GetDefaultCompany();

        // Sample 1: Simple Order (~1 page)
        var simpleOrder = new Order
        {
            Id = "ord-simple-001",
            OrderNumber = "ORD-2026-00125",
            OrderDate = new DateTimeOffset(2026, 9, 20, 10, 30, 0, TimeSpan.Zero),
            Status = OrderStatus.Confirmed,
            Currency = CurrencyCode.USD,
            Company = company,
            PaymentMethod = "Corporate Visa (ending 8821)",
            CustomerNotes = "Please deliver during business hours (9 AM - 5 PM).",
            Customer = new Customer
            {
                Id = "CUST-101",
                Name = "Sarah Jenkins",
                CompanyName = "Contoso Technologies LLC",
                Email = "s.jenkins@contoso.com",
                Phone = "+1 (415) 555-0142",
                BillingAddress = new Address
                {
                    Street = "500 Howard Street",
                    SuiteOrApt = "Floor 12",
                    City = "San Francisco",
                    State = "CA",
                    PostalCode = "94105",
                    Country = "United States"
                },
                ShippingAddress = new Address
                {
                    Street = "500 Howard Street",
                    SuiteOrApt = "Receiving Dock B",
                    City = "San Francisco",
                    State = "CA",
                    PostalCode = "94105",
                    Country = "United States"
                }
            },
            ShippingFee = 25.00m,
            AdditionalOrderDiscount = 0m,
            Items =
            [
                new OrderItem
                {
                    SKU = "PRD-SRV-001",
                    ProductName = "Enterprise Cloud Server Pro",
                    Description = "64-Core AMD EPYC, 256GB ECC RAM, 4TB NVMe SSD",
                    Quantity = 2,
                    UnitPrice = 1850.00m,
                    Discount = 0m,
                    TaxRate = 8.5m
                },
                new OrderItem
                {
                    SKU = "PRD-NET-012",
                    ProductName = "Managed 10GbE Switch 24-Port",
                    Description = "L3 Managed Rackmount Switch with PoE+ support",
                    Quantity = 1,
                    UnitPrice = 650.00m,
                    Discount = 0m,
                    TaxRate = 8.5m
                },
                new OrderItem
                {
                    SKU = "PRD-ACC-045",
                    ProductName = "Cat6A Shielded Patch Cable 10-Pack",
                    Description = "10ft High Performance 10Gbps Ethernet Cables",
                    Quantity = 4,
                    UnitPrice = 45.00m,
                    Discount = 0m,
                    TaxRate = 8.5m
                }
            ]
        };

        // Sample 2: Discounted Order (~1-2 pages)
        var discountedOrder = new Order
        {
            Id = "ord-discount-002",
            OrderNumber = "ORD-2026-00126",
            OrderDate = new DateTimeOffset(2026, 9, 21, 14, 15, 0, TimeSpan.Zero),
            Status = OrderStatus.Processing,
            Currency = CurrencyCode.USD,
            Company = company,
            PaymentMethod = "Direct ACH Wire Transfer",
            CustomerNotes = "Quarterly volume discount applied per Master Services Agreement MSA-2026-99.",
            Customer = new Customer
            {
                Id = "CUST-202",
                Name = "David Rodriguez",
                CompanyName = "Fabrikam Global Logistics",
                Email = "d.rodriguez@fabrikam.com",
                Phone = "+1 (312) 555-0188",
                BillingAddress = new Address
                {
                    Street = "233 S Wacker Dr",
                    SuiteOrApt = "Suite 3400",
                    City = "Chicago",
                    State = "IL",
                    PostalCode = "60606",
                    Country = "United States"
                },
                ShippingAddress = new Address
                {
                    Street = "1400 E Higgins Rd",
                    SuiteOrApt = "Warehouse Bay 4",
                    City = "Des Plaines",
                    State = "IL",
                    PostalCode = "60018",
                    Country = "United States"
                }
            },
            ShippingFee = 150.00m,
            AdditionalOrderDiscount = 200.00m,
            Items =
            [
                new OrderItem
                {
                    SKU = "DEV-WRK-100",
                    ProductName = "Developer Workstation X9",
                    Description = "Liquid Cooled, 32-Core CPU, 128GB RAM, RTX 4090",
                    Quantity = 4,
                    UnitPrice = 3200.00m,
                    Discount = 400.00m,
                    TaxRate = 7.0m
                },
                new OrderItem
                {
                    SKU = "MON-4K-027",
                    ProductName = "UltraSharp 32\" 4K HDR Monitor",
                    Description = "IPS Black Panel, 99% DCI-P3, USB-C 90W Hub",
                    Quantity = 8,
                    UnitPrice = 720.00m,
                    Discount = 160.00m,
                    TaxRate = 7.0m
                },
                new OrderItem
                {
                    SKU = "ACC-DSK-009",
                    ProductName = "Thunderbolt 4 Docking Station",
                    Description = "Dual 4K Display, Gigabit Ethernet, 100W Power Delivery",
                    Quantity = 4,
                    UnitPrice = 240.00m,
                    Discount = 40.00m,
                    TaxRate = 7.0m
                },
                new OrderItem
                {
                    SKU = "LIC-SFT-ENT",
                    ProductName = "Enterprise IDE Annual License",
                    Description = "Per-seat enterprise subscription with AI features",
                    Quantity = 10,
                    UnitPrice = 450.00m,
                    Discount = 500.00m,
                    TaxRate = 0.0m // Digital/Tax exempt
                }
            ]
        };

        // Sample 3: Large Order (60+ items, multi-page)
        var largeOrder = new Order
        {
            Id = "ord-large-003",
            OrderNumber = "ORD-2026-00127",
            OrderDate = new DateTimeOffset(2026, 9, 22, 9, 0, 0, TimeSpan.Zero),
            Status = OrderStatus.Confirmed,
            Currency = CurrencyCode.USD,
            Company = company,
            PaymentMethod = "Net-30 Invoice / Purchase Order PO-98412",
            CustomerNotes = "Multi-department campus hardware rollout. Please stage shipments by building tag.",
            Customer = new Customer
            {
                Id = "CUST-303",
                Name = "Elena Rostova",
                CompanyName = "Northwind Autonomous Systems",
                Email = "procurement@northwind.io",
                Phone = "+1 (512) 555-0177",
                BillingAddress = new Address
                {
                    Street = "11100 Metric Blvd",
                    SuiteOrApt = "Building 7",
                    City = "Austin",
                    State = "TX",
                    PostalCode = "78758",
                    Country = "United States"
                },
                ShippingAddress = new Address
                {
                    Street = "11100 Metric Blvd",
                    SuiteOrApt = "Central Receiving Facility",
                    City = "Austin",
                    State = "TX",
                    PostalCode = "78758",
                    Country = "United States"
                }
            },
            ShippingFee = 450.00m,
            AdditionalOrderDiscount = 500.00m,
            Items = GenerateLargeOrderItems(65)
        };

        return [simpleOrder, discountedOrder, largeOrder];
    }

    private static List<OrderItem> GenerateLargeOrderItems(int count)
    {
        var productCatalog = new (string Sku, string Name, string Desc, decimal Price, decimal Tax)[]
        {
            ("SRV-BLD-01", "Edge Compute Blade Server", "16-Core ARM64, 64GB LPDDR5, 1TB NVMe", 890.00m, 8.25m),
            ("NET-SFP-10", "10G SFP+ Optical Transceiver", "850nm MMF Multi-mode up to 300m", 65.00m, 8.25m),
            ("SEN-LID-4K", "Solid-State LiDAR Sensor 4K", "120deg FOV, 200m range, IP67 enclosure", 1450.00m, 8.25m),
            ("ROB-ARM-J2", "Industrial Servo Joint Actuator", "High torque harmonic drive with absolute encoder", 380.00m, 8.25m),
            ("PWR-UPS-3K", "Online Double-Conversion UPS 3000VA", "Pure Sine Wave, 2U Rackmount, SNMP card", 920.00m, 8.25m),
            ("CAB-FIB-LC", "LC to LC Duplex OM4 Fiber Cable 5m", "Laser-Optimized Aqua Jacket, Low insertion loss", 28.50m, 8.25m),
            ("ENC-RACK-42", "42U Server Rack Enclosure", "Perforated Mesh Doors, Cable Management Channels", 1150.00m, 8.25m),
            ("SW-CAM-HD", "Industrial PoE Vision Camera", "Global Shutter 5MP, 60fps, C-Mount Lens", 420.00m, 8.25m),
            ("DEV-KBD-MECH", "Rugged Industrial Mechanical Keyboard", "IP65 Waterproof, Cherry MX Black Switches", 145.00m, 8.25m),
            ("MEM-DDR5-64", "64GB DDR5-5600 ECC Registered RDIMM", "Enterprise Server Memory Module", 260.00m, 8.25m)
        };

        var items = new List<OrderItem>(count);
        for (int i = 1; i <= count; i++)
        {
            var template = productCatalog[(i - 1) % productCatalog.Length];
            int qty = (i % 5) + 1;
            decimal discount = (i % 4 == 0) ? 25.00m * qty : 0m;

            items.Add(new OrderItem
            {
                SKU = $"{template.Sku}-{i:D3}",
                ProductName = $"{template.Name} (Batch #{i:D2})",
                Description = $"{template.Desc} [Tag: ASSET-{1000 + i}]",
                Quantity = qty,
                UnitPrice = template.Price,
                Discount = discount,
                TaxRate = template.Tax
            });
        }

        return items;
    }
}
