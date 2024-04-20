using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OnlineJwellery_Shopping.Models;
using OnlineJwellery_Shopping.Heplers;
using BCrypt.Net;
using OnlineJwellery_Shopping.Data;

namespace OnlineJwellery_Shopping.Models
{

    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new JwelleryShoppingContext(
                serviceProvider.GetRequiredService<DbContextOptions<JwelleryShoppingContext>>()))
            {

                // Look for any existing data.
                if (context.User.Any() || context.Category.Any() || context.Blog.Any() || context.Brand.Any() ||
                context.Favorite.Any() || context.GoldAge.Any() || context.Order.Any() || context.OrderProduct.Any() ||
                context.Product.Any() || context.Review.Any() || context.OrderCancel.Any() || context.OrderReturn.Any() ||
                context.ReturnImages.Any())
                {
                    return;   // Database has been seeded
                }


                // Seed data for User
                var users = new User[]
                   {
                    new User
                    {
                        Username = "admin",
                        Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
                        Email = "admin@gmail.com",
                        Role = "Admin",
                        AccountBalance = 10000,
                        Thumbnail = "/images/person_1.jpg",
                        PhoneNumber = "1234567890",
                        Address = "123 Main Street, City, Country"
                    },
                    new User
                    {
                        Username = "user1",
                        Password = BCrypt.Net.BCrypt.HashPassword("123456789"),
                        Email = "user1@gmail.com",
                        Role = "User",
                        AccountBalance = 10000,
                        Thumbnail = "/images/person_2.jpg",
                        PhoneNumber = "9876543210",
                        Address = "456 Oak Street, City, Country"
                    },
                    new User
                    {
                        Username = "user2",
                        Password = BCrypt.Net.BCrypt.HashPassword("123456789"),
                        Email = "user2@gmail.com",
                        Role = "User",
                        AccountBalance = 10000,
                        Thumbnail = "/images/person_3.jpg",
                        PhoneNumber = "5555555555",
                        Address = "789 Pine Street, City, Country"
                    }
                   };
                foreach (var user in users)
                {
                    context.User.Add(user);
                }
                context.SaveChanges();

                // Seed data for Category
                var random = new Random();
                var categoryNames = new List<string> { "Rings", "Necklaces", "Bracelets", "Earrings", "Pendants", "Brooches", "Anklets", "Chains", "Charms" };

                foreach (var categoryName in categoryNames)
                {
                    var slug = SlugHelper.GenerateSlug(categoryName, categoryNames.IndexOf(categoryName) + 1); // Truyền thêm biến index của category
                    var category = new Category
                    {
                        CategoryName = categoryName,
                        Slug = slug
                    };
                    context.Category.Add(category);
                }
                context.SaveChanges();


                // Seed data for Brand

                var brandNames = new List<string> { "DiamondLux", "GemstoneGlow", "JewelCraft", "CrystalShine", "PreciousStone", "EternalSparkle", "LuxuryGems", "RoyalJewels", "OpulentOrnaments" };

                foreach (var brandName in brandNames)
                {
                    var brand = new Brand
                    {
                        BrandName = brandName
                    };
                    context.Brand.Add(brand);
                }
                context.SaveChanges();

                // Seed data for GoldAge

                var goldAges = new List<string> { "10k", "14K", "18K", "24K" };

                foreach (var age in goldAges)
                {
                    var goldAge = new GoldAge
                    {
                        Age = age
                    };
                    context.GoldAge.Add(goldAge);
                }
                context.SaveChanges();

                // Seed data for Product
                var categories = context.Category.ToList();
                var brands = context.Brand.ToList();
                var goldAges1 = context.GoldAge.ToList();
                var thumbnailPaths = Enumerable.Range(1, 80).Select(i => $"/images/product-{i}.jpg").ToList();

                // Danh sách tên và mô tả sản phẩm
                var productNames = new List<string>{
            "Elegant Diamond Necklace",   "Sapphire Elegance Ring",
            "Emerald Sparkle Bracelet",   "Ruby Radiance Earrings",
            "Pearl Perfection Pendant",   "Amethyst Adorned Tiara",
            "Topaz Treasure Brooch",      "Aquamarine Aura Anklet",
            "Opal Opulence Choker",       "Diamond Delight Engagement Ring",
            "Sapphire Symphony Necklace", "Emerald Enchantment Bracelet",
            "Ruby Romance Earrings",      "Pearl Passion Pendant",
            "Amethyst Amore Ring",        "Topaz Twilight Brooch",
            "Aquamarine Allure Tiara",    "Opal Odyssey Necklace",
            "Diamond Dazzle Bracelet",    "Sapphire Serenade Earrings",
            "Emerald Elegance Pendant",   "Ruby Rapture Ring",
            "Pearl Princess Tiara",       "Amethyst Affection Brooch",
            "Topaz Temptation Anklet",    "Aquamarine Adventure Necklace",
            "Opal Obsession Bracelet",    "Diamond Dream Earrings",
            "Sapphire Splendor Pendant",  "Emerald Essence Ring" };

                var stoneTypes = new List<string> { "Diamond", "Sapphire", "Emerald", "Ruby", "Pearl", "Amethyst", "Topaz", "Aquamarine", "Opal" };
                var colors = new List<string> { "White", "Green", "Orange", "Red", "Yellow", "Purple", "Blue", "Pink", "Multicolor" };
                var sizes = new List<string> { "6.50 - 6.55 x 3.92 mm", "2.50 - 9.55 x 6 mm", "7.20 - 8.55 x 2.92 mm", "3.50 - 6.55 x 3.92 mm", "6.50 - 2.55 x 3.4 mm", "7.50 - 7.55 x 7.92 mm" };
                var materials = new List<string> { "Gold", "Silver", "Platinum", "Rose Gold", "White Gold", "Titanium" };

                for (int i = 1; i < 100; i++)

                {

                    var productName = productNames[random.Next(productNames.Count)];
                    var stoneType = stoneTypes[random.Next(stoneTypes.Count)];
                    var color = colors[random.Next(colors.Count)];
                    var size = sizes[random.Next(sizes.Count)];
                    var material = materials[random.Next(materials.Count)];
                    var slug = SlugHelper.GenerateSlug(productName, i);


                    var product = new Product
                    {
                        ProductName = productName,
                        Slug = slug,
                        StoneType = stoneType,
                        Color = color,
                        Size = size,
                        Material = material,
                        Price = random.Next(100, 1000),
                        TotalWeight = random.Next(1, 10),
                        Thumbnail = thumbnailPaths[i % 80],
                        SmallThumbnail1 = thumbnailPaths[i % 80],
                        SmallThumbnail2 = thumbnailPaths[(i + 1) % 80],
                        SmallThumbnail3 = thumbnailPaths[(i + 2) % 80],
                        SmallThumbnail4 = thumbnailPaths[(i + 3) % 80],
                        Qty = random.Next(1, 50),
                        CertificationCode = $"CERT-{i}-DTS",
                        CategoryId = categories[random.Next(categories.Count)].CategoryId,
                        BrandId = brands[random.Next(brands.Count)].BrandId,
                        GoldAgeId = goldAges1[random.Next(goldAges1.Count)].GoldAgeId,

                    };
                    context.Product.Add(product);
                }
                context.SaveChanges();



                // Seed data for Order 
                var orderStatuses = new string[] { "pending", "confirmed", "shipping", "shipped", "complete", "cancel" };
                var isPaidOptions = new string[] { "unpaid", "paid" };


                var provinces = new string[] { "Hồ Chí Minh", "Hà Nội", "Đà Nẵng", "Cần Thơ", "Hải Phòng", "An Giang" };
                var districts = new string[] { "Quận 1", "Quận 2", "Quận 3", "Quận 4", "Quận 5", "Quận 6" };
                var wards = new string[] { "Phường 1", "Phường 2", "Phường 3", "Phường 4", "Phường 5", "Phường 6" };
                var AddressDetails = new string[] { "nhà 999 đường 99", "nhà 88 đường 2" };

                var FullNames = new string[] { "Tien Dung", "Quang Long" };
                var Emails = new string[] { "dung@gmail.com", "long@gmail.com" };
                var Telephones = new string[] { "09828781262", "09275741287" };

                var paymentMethods = new string[] { "COD", "Paypal" };
                var shippingMethods = new string[] { "FreeShipping", "Express" };


                for (int i = 1; i <= 10; i++)
                {
                    var order = new Order
                    {
                        UserId = random.Next(1, users.Length + 1),
                        OrderDate = DateTime.Now.AddSeconds(-i).AddMilliseconds(-DateTime.Now.Millisecond),
                        TotalAmount = random.Next(1, 101),
                        Status = orderStatuses[random.Next(orderStatuses.Length)],
                        IsPaid = isPaidOptions[random.Next(isPaidOptions.Length)],


                        // Thêm thông tin địa chỉ
                        Province = provinces[random.Next(provinces.Length)],
                        District = districts[random.Next(districts.Length)],
                        Ward = wards[random.Next(wards.Length)],
                        AddressDetail = AddressDetails[random.Next(AddressDetails.Length)],

                        // Random thông tin người dùng
                        FullName = FullNames[random.Next(FullNames.Length)],
                        Email = Emails[random.Next(Emails.Length)],
                        Telephone = Telephones[random.Next(Telephones.Length)],
                        // Random phương thức thanh toán và vận chuyển
                        PaymentMethod = paymentMethods[random.Next(paymentMethods.Length)],
                        ShippingMethod = shippingMethods[random.Next(shippingMethods.Length)]
                    };
                    context.Order.Add(order);
                }
                context.SaveChanges();

                // Seed data for OrderProduct
                for (int i = 1; i <= 10; i++)
                {
                    var orderProduct = new OrderProduct
                    {
                        OrderId = random.Next(1, 10),
                        ProductId = random.Next(1, 10),
                        Qty = random.Next(1, 5),
                        Price = random.Next(1, 101),
                        Status = random.Next(2) // Randomly assigns 0 or 1
                    };
                    context.OrderProduct.Add(orderProduct);
                }
                context.SaveChanges();

                // Seed data for OrderCancel
                var cancelReasons = new List<string>
                {
                  "Out of stock",
                  "Customer changed mind",
                  "Item damaged during shipping",
                  "Duplicate order",
                  "Payment issue",
                  "Incorrect address provided",
                  "Item not as described",
                  "Customer request"
                };

                for (int i = 1; i <= 10; i++)
                {
                    var orderCancel = new OrderCancel
                    {
                        OrderId = random.Next(1, 10),
                        Reason = cancelReasons[random.Next(cancelReasons.Count)]
                    };
                    context.OrderCancel.Add(orderCancel);
                }
                context.SaveChanges();

                // Seed data for OrderReturn
                var rerurnStatuses = new string[] { "pending", "approved", "rejected" };
                var returnReasons = new List<string>
                {
                  "Item damaged during shipping",
                  "Incorrect item received",
                  "Item not as described",
                  "Customer changed mind",
                  "Duplicate order",
                  "Payment issue",
                  "Other"
                };

                for (int i = 1; i <= 10; i++)
                {
                    var orderReturn = new OrderReturn
                    {
                        OrderId = random.Next(1, 10),
                        ProductId = random.Next(1, 10),
                        UserId = random.Next(1, users.Length + 1),
                        ReturnDate = DateTime.Now.AddDays(-i),
                        Reason = returnReasons[random.Next(returnReasons.Count)],
                        Status = rerurnStatuses[random.Next(rerurnStatuses.Length)],
                        Description = "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
                        RejectReason = "bla",
                        RefundAmount = random.Next(100, 1001)
                    };
                    context.OrderReturn.Add(orderReturn);
                }

                context.SaveChanges();

                // Seed data for ReturnImages
                var returnImagePaths = Enumerable.Range(1, 80).Select(i => $"/images/product-{i}.jpg").ToList();
                for (int i = 1; i <= 10; i++)
                {
                    var returnImage = new ReturnImages
                    {
                        OrderReturnId = random.Next(1, 10),
                        ImagePath = returnImagePaths[i % 80]
                    };
                    context.ReturnImages.Add(returnImage);
                }
                context.SaveChanges();




                // Seed data for Favorite
                for (int i = 1; i <= 10; i++)
                {
                    var favorite = new Favorite
                    {
                        ProductName = productNames[random.Next(productNames.Count)],
                        Price = random.Next(1, 101),
                        Thumbnail = thumbnailPaths[i % 80], // Lặp lại đường dẫn ảnh sau mỗi 10 sản phẩm

                        // Thêm thông tin người dùng và sản phẩm
                        User = users[random.Next(users.Length)],
                        Product = context.Product.Find(random.Next(1, 101))
                    };
                    context.Favorite.Add(favorite);
                }
                context.SaveChanges();

                // Seed data for Review
                var productIds = context.Product.Select(p => p.ProductId).ToList();
                var userIds = context.User.Select(u => u.UserId).ToList();
                var reviewStatuses = new string[] { "pending", "approved", "rejected" }; // Mảng các trạng thái đánh giá

                for (int i = 1; i <= 10; i++)
                {
                    var review = new Review
                    {
                        ProductId = productIds[random.Next(productIds.Count)],
                        UserId = userIds[random.Next(userIds.Count)],
                        Comment = $"This product is amazing! Highly recommended.",
                        RatingValue = random.Next(1, 5),
                        ReviewDate = DateTime.Now.AddDays(-i),
                        Status = reviewStatuses[random.Next(reviewStatuses.Length)] // Chọn ngẫu nhiên một trạng thái từ mảng reviewStatuses
                    };
                    context.Review.Add(review);
                }
                context.SaveChanges();



                // seed data for Blog
                var blogTitles = new List<string> {
    "The Fascinating World of Gemstone Jewelry",
    "Exploring the Beauty of Diamond Jewelry",
    "Emeralds: The Gemstone of Spring",
    "Sapphire: The Gemstone of Wisdom and Royalty",
    "Ruby: The Gemstone of Passion and Love",
    "Pearls: Timeless Elegance in Jewelry",
    "Amethyst: The Gemstone of Tranquility",
    "Topaz: A Glittering November Birthstone",
    "Aquamarine: The Gemstone of Serenity",
    "Opals: The Enigmatic Gemstone of Mystery and Magic"
};

                var blogContents = new List<string> {
    "Gemstone jewelry has been adored for centuries for its vibrant colors and inherent beauty. From ancient civilizations to modern-day fashion, gemstones have captivated people with their allure and symbolism.",
    "Diamonds are the ultimate symbol of luxury and prestige. Their brilliance and durability make them the most sought-after gemstone for engagement rings, necklaces, earrings, and more.",
    "As spring blooms, so does the allure of emerald jewelry. Known for its lush green hue, emerald is a gemstone associated with rebirth, renewal, and growth. Explore the beauty of emerald jewelry and its significance.",
    "Sapphires have long been revered for their deep blue hue and association with wisdom and royalty. Discover the allure of sapphire jewelry, from classic engagement rings to stunning necklaces and earrings.",
    "Rubies have a rich history steeped in passion and love. Their fiery red color symbolizes vitality, energy, and romance. Learn about the significance of ruby jewelry and its timeless appeal.",
    "Pearls are the epitome of timeless elegance. From classic pearl necklaces to modern designs, pearl jewelry adds sophistication and grace to any ensemble. Explore the world of pearl jewelry and its enduring charm.",
    "Amethyst is a gemstone prized for its soothing purple hue and calming properties. From dainty amethyst earrings to bold statement pieces, discover the beauty and significance of amethyst jewelry.",
    "Topaz, the birthstone of November, dazzles with its range of colors, from warm golden hues to cool blues. Learn about the symbolism and versatility of topaz jewelry in this comprehensive guide.",
    "Aquamarine, with its tranquil blue color reminiscent of the ocean, is a gemstone associated with serenity, clarity, and inner peace. Explore the allure of aquamarine jewelry and its calming effects.",
    "Opals are unique gemstones renowned for their iridescent play-of-color, which mesmerizes and captivates the beholder. Discover the mystique and magic of opal jewelry in this exploration of one of nature's most enigmatic gems."
};

                var blogTag = new List<string> {"hello", "new", "best" , "bla" , "bedd",
                                 "another", "tag", "example", "tags", "here"};

                var blogThumbnailPaths = Enumerable.Range(1, 10).Select(i => $"/images/product-{i}.jpg").ToList();

                for (int i = 0; i < blogTitles.Count; i++)
                {
                    var blog = new Blog
                    {
                        Title = blogTitles[i],
                        Content = blogContents[i],
                        Tag = blogTag[i],
                        Thumbnail = blogThumbnailPaths[i],
                        BlogDate = DateTime.Now.AddDays(-i)
                    };
                    context.Blog.Add(blog);
                }
                context.SaveChanges();
            }
        }
    }
}



