using ProductService.Model;

namespace ProductService.Data;
public static class DataSeeder
{
    public static void Seed(AppDbContext context)
    {
        if (!context.Products.Any())
        {
            context.Products.AddRange(
                new Product
                {
                    Name = "iPhone 17",
                    Description = "Latest Apple smartphone with advanced chipset and improved camera system",
                    Price = 1299.99m,
                    Category = "Smartphones",
                    ImageUrl = "https://idealz.lk/wp-content/uploads/2025/09/iphone-17-pro-cosmicorange.jpg"
                },
                new Product
                {
                    Name = "Anker R50i",
                    Description = "True wireless earbuds with deep bass and long battery life",
                    Price = 49.99m,
                    Category = "Audio",
                    ImageUrl = "https://www.simplytek.lk/cdn/shop/files/anker-soundcore-r50i-true-wireless-earbuds-simplytek-lk-sri-lanka_1.webp?v=1712722630&width=1220"
                },
                new Product
                {
                    Name = "Redmi 12",
                    Description = "Affordable smartphone with large display and reliable performance",
                    Price = 199.99m,
                    Category = "Smartphones",
                    ImageUrl = "https://i02.appmifile.com/80_operatorx_operatorx_opx/25/07/2023/df6fa0925b5fb5393d61310fa54fd7e7.png"
                },
                new Product
                {
                    Name = "Samsung Galaxy S24",
                    Description = "Flagship Samsung smartphone with high resolution camera and AI features",
                    Price = 999.99m,
                    Category = "Smartphones",
                    ImageUrl = "https://banana.lk/wp-content/uploads/2024/08/galaxy-s24-ultra-gray.jpg"
                },
                new Product
                {
                    Name = "JBL Tune 680NC",
                    Description = "Hi-Res JBL Pure Bass Sound plus Spatial Sound Adaptive Noise Cancelling with Smart Ambient 2-mic perfect calls with beamforming technology Up to 76 hours of battery life",
                    Price = 90m,
                    Category = "Audio",
                    ImageUrl = "https://www.simplytek.lk/cdn/shop/files/JBL_Tune_680NC_Simplytek-lk-sri-lanka_2.jpg?v=1765179439"
                },
                new Product
                {
                    Name = "GoPro HERO10 Black",
                    Description = "23MP GP2 Chip with Improved Performance",
                    Price = 300m,
                    Category = "Camera",
                    ImageUrl = "https://goprolanka.lk/wp-content/uploads/2024/01/1631822665_IMG_1606161.jpg"
                });
                context.SaveChanges();
        } 
        
    }
}