using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "car_brands",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__car_bran__3213E83FC4C36A9B", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "car_categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__car_cate__3213E83FE7D737A6", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "car_extras",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    pricing_type = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__car_extr__3213E83F91415630", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "coupons",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    discount_type = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    discount_value = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    max_usage = table.Column<int>(type: "int", nullable: true),
                    usage_count = table.Column<int>(type: "int", nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__coupons__3213E83FA0A5561E", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "flights",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    flight_number = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    carrier_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    origin_airport_code = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    origin_city = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    destination_airport_code = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false),
                    destination_city = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    departure_at_utc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    arrival_at_utc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    duration_minutes = table.Column<int>(type: "int", nullable: true),
                    cabin_class = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false, defaultValue: "economy"),
                    base_price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    currency = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false, defaultValue: "USD"),
                    seats_available = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false, defaultValue: "scheduled"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__flights__3213E83F33D7A2D7", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "locations",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    city = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    address_line = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    latitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true),
                    longitude = table.Column<decimal>(type: "decimal(9,6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__location__3213E83F32EDE253", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__roles__3213E83F247A45DA", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cars",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    brand_id = table.Column<int>(type: "int", nullable: false),
                    car_category_id = table.Column<int>(type: "int", nullable: false),
                    model = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    year = table.Column<int>(type: "int", nullable: true),
                    seats_count = table.Column<int>(type: "int", nullable: false),
                    transmission = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    fuel_type = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    pickup_location_id = table.Column<int>(type: "int", nullable: true),
                    dropoff_location_id = table.Column<int>(type: "int", nullable: true),
                    price_per_day = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    status = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false, defaultValue: "draft"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__cars__3213E83FD29F8504", x => x.id);
                    table.ForeignKey(
                        name: "FK_cars_brand",
                        column: x => x.brand_id,
                        principalTable: "car_brands",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_cars_category",
                        column: x => x.car_category_id,
                        principalTable: "car_categories",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_cars_dropoff_location",
                        column: x => x.dropoff_location_id,
                        principalTable: "locations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_cars_pickup_location",
                        column: x => x.pickup_location_id,
                        principalTable: "locations",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "hotels",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    location_id = table.Column<int>(type: "int", nullable: false),
                    main_image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    star_rating = table.Column<byte>(type: "tinyint", nullable: true),
                    check_in_time = table.Column<TimeOnly>(type: "time", nullable: true),
                    check_out_time = table.Column<TimeOnly>(type: "time", nullable: true),
                    status = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false, defaultValue: "draft"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__hotels__3213E83F0A769D6A", x => x.id);
                    table.ForeignKey(
                        name: "FK_hotels_location",
                        column: x => x.location_id,
                        principalTable: "locations",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tours",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    slug = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    summary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    full_description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    main_image_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    duration_days = table.Column<int>(type: "int", nullable: true),
                    location_id = table.Column<int>(type: "int", nullable: true),
                    difficulty = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: true),
                    status = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    is_deleted = table.Column<bool>(type: "bit", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    deleted_by = table.Column<long>(type: "bigint", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    cancelled_by = table.Column<long>(type: "bigint", nullable: true),
                    cancellation_reason_type = table.Column<int>(type: "int", nullable: true),
                    cancellation_reason_details = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__tours__3213E83F0E0F030A", x => x.id);
                    table.ForeignKey(
                        name: "FK_tours_location",
                        column: x => x.location_id,
                        principalTable: "locations",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "passengers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    role_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    location_id = table.Column<int>(type: "int", nullable: true),
                    is_email_verified = table.Column<bool>(type: "bit", nullable: false),
                    refreshToken = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    refresh_token_expiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    status = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false, defaultValue: "unverified"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__users__3213E83F803EACAC", x => x.id);
                    table.ForeignKey(
                        name: "FK_users_location",
                        column: x => x.location_id,
                        principalTable: "locations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_users_role",
                        column: x => x.role_id,
                        principalTable: "roles",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "car_images",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    car_id = table.Column<long>(type: "bigint", nullable: false),
                    url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__car_imag__3213E83F1A84F5DE", x => x.id);
                    table.ForeignKey(
                        name: "FK_car_images_car",
                        column: x => x.car_id,
                        principalTable: "cars",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "car_pricing_tiers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    car_id = table.Column<long>(type: "bigint", nullable: false),
                    from_hours = table.Column<int>(type: "int", nullable: false),
                    to_hours = table.Column<int>(type: "int", nullable: true),
                    price_per_hour = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__car_pric__3213E83F05E65D4C", x => x.id);
                    table.ForeignKey(
                        name: "FK_car_pricing_tiers_car",
                        column: x => x.car_id,
                        principalTable: "cars",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hotel_images",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    hotel_id = table.Column<long>(type: "bigint", nullable: false),
                    url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__hotel_im__3213E83F5E313734", x => x.id);
                    table.ForeignKey(
                        name: "FK_hotel_images_hotel",
                        column: x => x.hotel_id,
                        principalTable: "hotels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "rooms",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    hotel_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    bed_type = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: true),
                    occupancy_adults = table.Column<int>(type: "int", nullable: false, defaultValue: 2),
                    occupancy_children = table.Column<int>(type: "int", nullable: false),
                    price_per_night = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    refundable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    status = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false, defaultValue: "draft"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__rooms__3213E83FA2911DD9", x => x.id);
                    table.ForeignKey(
                        name: "FK_rooms_hotel",
                        column: x => x.hotel_id,
                        principalTable: "hotels",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tour_images",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tour_id = table.Column<long>(type: "bigint", nullable: false),
                    url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__tour_ima__3213E83FA46D7C76", x => x.id);
                    table.ForeignKey(
                        name: "FK_tour_images_tour",
                        column: x => x.tour_id,
                        principalTable: "tours",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tour_inclusions",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tour_id = table.Column<long>(type: "bigint", nullable: false),
                    item_text = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    is_included = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__tour_inc__3213E83F53BCBFD2", x => x.id);
                    table.ForeignKey(
                        name: "FK_tour_inclusions_tour",
                        column: x => x.tour_id,
                        principalTable: "tours",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tour_price_tiers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tour_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    adult_price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    child_price = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    infant_price = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    currency = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false, defaultValue: "USD")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__tour_pri__3213E83FD09A27BF", x => x.id);
                    table.ForeignKey(
                        name: "FK_tour_price_tiers_tour",
                        column: x => x.tour_id,
                        principalTable: "tours",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "bookings",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    booking_number = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    category = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    status = table.Column<int>(type: "int", unicode: false, maxLength: 10, nullable: false),
                    IsCancelled = table.Column<bool>(type: "bit", nullable: true),
                    cancelled_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    cancellation_reason_type = table.Column<int>(type: "int", nullable: true),
                    cancellation_reason_details = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    subtotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    discount_amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    total_price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    currency = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false, defaultValue: "USD"),
                    payment_status = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false, defaultValue: "unpaid"),
                    coupon_id = table.Column<int>(type: "int", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    CreatedBy = table.Column<long>(type: "bigint", nullable: false),
                    UpdatedBy = table.Column<long>(type: "bigint", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__bookings__3213E83F6EE64705", x => x.id);
                    table.ForeignKey(
                        name: "FK_bookings_coupon",
                        column: x => x.coupon_id,
                        principalTable: "coupons",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_bookings_user",
                        column: x => x.user_id,
                        principalTable: "passengers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "favorites",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    category = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    item_id = table.Column<long>(type: "bigint", nullable: false),
                    added_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__favorite__3213E83F3AE46643", x => x.id);
                    table.ForeignKey(
                        name: "FK_favorites_user",
                        column: x => x.user_id,
                        principalTable: "passengers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "room_availability",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    room_id = table.Column<long>(type: "bigint", nullable: false),
                    date = table.Column<DateOnly>(type: "date", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    price_override = table.Column<decimal>(type: "decimal(10,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__room_ava__3213E83F2D19570C", x => x.id);
                    table.ForeignKey(
                        name: "FK_room_availability_room",
                        column: x => x.room_id,
                        principalTable: "rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "room_extras",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    room_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    price = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__room_ext__3213E83F4B5F7045", x => x.id);
                    table.ForeignKey(
                        name: "FK_room_extras_room",
                        column: x => x.room_id,
                        principalTable: "rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "room_images",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    room_id = table.Column<long>(type: "bigint", nullable: false),
                    url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    sort_order = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__room_ima__3213E83F658ACE73", x => x.id);
                    table.ForeignKey(
                        name: "FK_room_images_room",
                        column: x => x.room_id,
                        principalTable: "rooms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tour_schedules",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    tour_id = table.Column<long>(type: "bigint", nullable: false),
                    price_tier_id = table.Column<long>(type: "bigint", nullable: false),
                    start_date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    capacity = table.Column<int>(type: "int", nullable: false),
                    available_slots = table.Column<int>(type: "int", nullable: false),
                    is_cancelled = table.Column<bool>(type: "bit", nullable: false),
                    cancelled_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    cancellation_reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__tour_sch__3213E83FF02F5E8A", x => x.id);
                    table.ForeignKey(
                        name: "FK_tour_schedules_price_tier",
                        column: x => x.price_tier_id,
                        principalTable: "tour_price_tiers",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_tour_schedules_tour",
                        column: x => x.tour_id,
                        principalTable: "tours",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "car_bookings",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    booking_id = table.Column<long>(type: "bigint", nullable: false),
                    car_id = table.Column<long>(type: "bigint", nullable: false),
                    pickup_location_id = table.Column<int>(type: "int", nullable: false),
                    dropoff_location_id = table.Column<int>(type: "int", nullable: false),
                    pickup_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    dropoff_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    driver_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__car_book__3213E83FE8D298FB", x => x.id);
                    table.ForeignKey(
                        name: "FK_car_bookings_booking",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_car_bookings_car",
                        column: x => x.car_id,
                        principalTable: "cars",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_car_bookings_dropoff_location",
                        column: x => x.dropoff_location_id,
                        principalTable: "locations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_car_bookings_pickup_location",
                        column: x => x.pickup_location_id,
                        principalTable: "locations",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "flight_bookings",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    booking_id = table.Column<long>(type: "bigint", nullable: false),
                    flight_id = table.Column<long>(type: "bigint", nullable: false),
                    return_flight_id = table.Column<long>(type: "bigint", nullable: true),
                    trip_type = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    price = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__flight_b__3213E83FA547652B", x => x.id);
                    table.ForeignKey(
                        name: "FK_flight_bookings_booking",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_flight_bookings_flight",
                        column: x => x.flight_id,
                        principalTable: "flights",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_flight_bookings_return_flight",
                        column: x => x.return_flight_id,
                        principalTable: "flights",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "hotel_bookings",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    booking_id = table.Column<long>(type: "bigint", nullable: false),
                    room_id = table.Column<long>(type: "bigint", nullable: false),
                    check_in_date = table.Column<DateOnly>(type: "date", nullable: false),
                    check_out_date = table.Column<DateOnly>(type: "date", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    guests_adults = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    guests_children = table.Column<int>(type: "int", nullable: false),
                    price_per_night = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__hotel_bo__3213E83F0F36DF05", x => x.id);
                    table.ForeignKey(
                        name: "FK_hotel_bookings_booking",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_hotel_bookings_room",
                        column: x => x.room_id,
                        principalTable: "rooms",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "payments",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    booking_id = table.Column<long>(type: "bigint", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    currency = table.Column<string>(type: "char(3)", unicode: false, fixedLength: true, maxLength: 3, nullable: false, defaultValue: "USD"),
                    gateway = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: false),
                    status = table.Column<string>(type: "varchar(15)", unicode: false, maxLength: 15, nullable: false, defaultValue: "pending"),
                    transaction_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__payments__3213E83FA5C35E5F", x => x.id);
                    table.ForeignKey(
                        name: "FK_payments_booking",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "reviews",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    booking_id = table.Column<long>(type: "bigint", nullable: true),
                    category = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false),
                    item_id = table.Column<long>(type: "bigint", nullable: false),
                    rating = table.Column<byte>(type: "tinyint", nullable: false),
                    title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "varchar(10)", unicode: false, maxLength: 10, nullable: false, defaultValue: "pending"),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__reviews__3213E83F474DF86E", x => x.id);
                    table.ForeignKey(
                        name: "FK_reviews_booking",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_reviews_user",
                        column: x => x.user_id,
                        principalTable: "passengers",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "tour_bookings",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    booking_id = table.Column<long>(type: "bigint", nullable: false),
                    tour_schedule_id = table.Column<long>(type: "bigint", nullable: false),
                    adults_count = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    children_count = table.Column<int>(type: "int", nullable: false),
                    infants_count = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__tour_boo__3213E83F469EEF5F", x => x.id);
                    table.ForeignKey(
                        name: "FK_tour_bookings_booking",
                        column: x => x.booking_id,
                        principalTable: "bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tour_bookings_schedule",
                        column: x => x.tour_schedule_id,
                        principalTable: "tour_schedules",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "car_booking_extras",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    car_booking_id = table.Column<long>(type: "bigint", nullable: false),
                    car_extra_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    price = table.Column<decimal>(type: "decimal(10,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__car_book__3213E83F3CA189D1", x => x.id);
                    table.ForeignKey(
                        name: "FK_car_booking_extras_booking",
                        column: x => x.car_booking_id,
                        principalTable: "car_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_car_booking_extras_extra",
                        column: x => x.car_extra_id,
                        principalTable: "car_extras",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "flight_booking_passengers",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    flight_booking_id = table.Column<long>(type: "bigint", nullable: false),
                    title = table.Column<string>(type: "varchar(5)", unicode: false, maxLength: 5, nullable: true),
                    first_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    passport_number = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__flight_b__3213E83F11541365", x => x.id);
                    table.ForeignKey(
                        name: "FK_flight_passengers_booking",
                        column: x => x.flight_booking_id,
                        principalTable: "flight_bookings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "created_at", "name" },
                values: new object[,]
                {
                    { 1, new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Passenger" },
                    { 2, new DateTime(2026, 7, 7, 0, 0, 0, 0, DateTimeKind.Utc), "Admin" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_coupon_id",
                table: "bookings",
                column: "coupon_id");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_user_status",
                table: "bookings",
                columns: new[] { "user_id", "status" });

            migrationBuilder.CreateIndex(
                name: "UQ_bookings_number",
                table: "bookings",
                column: "booking_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_car_booking_extras_car_booking_id",
                table: "car_booking_extras",
                column: "car_booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_car_booking_extras_car_extra_id",
                table: "car_booking_extras",
                column: "car_extra_id");

            migrationBuilder.CreateIndex(
                name: "IX_car_bookings_car_id",
                table: "car_bookings",
                column: "car_id");

            migrationBuilder.CreateIndex(
                name: "IX_car_bookings_dropoff_location_id",
                table: "car_bookings",
                column: "dropoff_location_id");

            migrationBuilder.CreateIndex(
                name: "IX_car_bookings_pickup_location_id",
                table: "car_bookings",
                column: "pickup_location_id");

            migrationBuilder.CreateIndex(
                name: "UQ_car_bookings_booking",
                table: "car_bookings",
                column: "booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_car_brands_name",
                table: "car_brands",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_car_categories_name",
                table: "car_categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_car_images_car_id",
                table: "car_images",
                column: "car_id");

            migrationBuilder.CreateIndex(
                name: "IX_car_pricing_tiers_car_id",
                table: "car_pricing_tiers",
                column: "car_id");

            migrationBuilder.CreateIndex(
                name: "IX_cars_brand_id",
                table: "cars",
                column: "brand_id");

            migrationBuilder.CreateIndex(
                name: "IX_cars_car_category_id",
                table: "cars",
                column: "car_category_id");

            migrationBuilder.CreateIndex(
                name: "IX_cars_dropoff_location_id",
                table: "cars",
                column: "dropoff_location_id");

            migrationBuilder.CreateIndex(
                name: "IX_cars_pickup_location_id",
                table: "cars",
                column: "pickup_location_id");

            migrationBuilder.CreateIndex(
                name: "UQ_coupons_code",
                table: "coupons",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_favorites",
                table: "favorites",
                columns: new[] { "user_id", "category", "item_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_flight_booking_passengers_flight_booking_id",
                table: "flight_booking_passengers",
                column: "flight_booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_flight_bookings_flight_id",
                table: "flight_bookings",
                column: "flight_id");

            migrationBuilder.CreateIndex(
                name: "IX_flight_bookings_return_flight_id",
                table: "flight_bookings",
                column: "return_flight_id");

            migrationBuilder.CreateIndex(
                name: "UQ_flight_bookings_booking",
                table: "flight_bookings",
                column: "booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_flights_route_departure",
                table: "flights",
                columns: new[] { "origin_airport_code", "destination_airport_code", "departure_at_utc" });

            migrationBuilder.CreateIndex(
                name: "IX_hotel_bookings_room_id",
                table: "hotel_bookings",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "UQ_hotel_bookings_booking",
                table: "hotel_bookings",
                column: "booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_hotel_images_hotel_id",
                table: "hotel_images",
                column: "hotel_id");

            migrationBuilder.CreateIndex(
                name: "IX_hotels_location_id",
                table: "hotels",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "UQ_hotels_slug",
                table: "hotels",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_passengers_location_id",
                table: "passengers",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "IX_passengers_role_id",
                table: "passengers",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_users_status",
                table: "passengers",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "UQ_users_email",
                table: "passengers",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_payments_booking",
                table: "payments",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_payments_transaction",
                table: "payments",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_booking_id",
                table: "reviews",
                column: "booking_id");

            migrationBuilder.CreateIndex(
                name: "IX_reviews_category_item",
                table: "reviews",
                columns: new[] { "category", "item_id" });

            migrationBuilder.CreateIndex(
                name: "IX_reviews_user",
                table: "reviews",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UQ_roles_name",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_room_availability",
                table: "room_availability",
                columns: new[] { "room_id", "date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_room_extras_room_id",
                table: "room_extras",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "IX_room_images_room_id",
                table: "room_images",
                column: "room_id");

            migrationBuilder.CreateIndex(
                name: "IX_rooms_hotel_id",
                table: "rooms",
                column: "hotel_id");

            migrationBuilder.CreateIndex(
                name: "IX_tour_bookings_tour_schedule_id",
                table: "tour_bookings",
                column: "tour_schedule_id");

            migrationBuilder.CreateIndex(
                name: "UQ_tour_bookings_booking",
                table: "tour_bookings",
                column: "booking_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tour_images_tour_id",
                table: "tour_images",
                column: "tour_id");

            migrationBuilder.CreateIndex(
                name: "IX_tour_inclusions_tour_id",
                table: "tour_inclusions",
                column: "tour_id");

            migrationBuilder.CreateIndex(
                name: "IX_tour_price_tiers_tour_id",
                table: "tour_price_tiers",
                column: "tour_id");

            migrationBuilder.CreateIndex(
                name: "IX_tour_schedules_price_tier_id",
                table: "tour_schedules",
                column: "price_tier_id");

            migrationBuilder.CreateIndex(
                name: "IX_tour_schedules_tour_start",
                table: "tour_schedules",
                columns: new[] { "tour_id", "start_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tours_location_id",
                table: "tours",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "UQ_tours_slug",
                table: "tours",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "car_booking_extras");

            migrationBuilder.DropTable(
                name: "car_images");

            migrationBuilder.DropTable(
                name: "car_pricing_tiers");

            migrationBuilder.DropTable(
                name: "favorites");

            migrationBuilder.DropTable(
                name: "flight_booking_passengers");

            migrationBuilder.DropTable(
                name: "hotel_bookings");

            migrationBuilder.DropTable(
                name: "hotel_images");

            migrationBuilder.DropTable(
                name: "payments");

            migrationBuilder.DropTable(
                name: "reviews");

            migrationBuilder.DropTable(
                name: "room_availability");

            migrationBuilder.DropTable(
                name: "room_extras");

            migrationBuilder.DropTable(
                name: "room_images");

            migrationBuilder.DropTable(
                name: "tour_bookings");

            migrationBuilder.DropTable(
                name: "tour_images");

            migrationBuilder.DropTable(
                name: "tour_inclusions");

            migrationBuilder.DropTable(
                name: "car_bookings");

            migrationBuilder.DropTable(
                name: "car_extras");

            migrationBuilder.DropTable(
                name: "flight_bookings");

            migrationBuilder.DropTable(
                name: "rooms");

            migrationBuilder.DropTable(
                name: "tour_schedules");

            migrationBuilder.DropTable(
                name: "cars");

            migrationBuilder.DropTable(
                name: "bookings");

            migrationBuilder.DropTable(
                name: "flights");

            migrationBuilder.DropTable(
                name: "hotels");

            migrationBuilder.DropTable(
                name: "tour_price_tiers");

            migrationBuilder.DropTable(
                name: "car_brands");

            migrationBuilder.DropTable(
                name: "car_categories");

            migrationBuilder.DropTable(
                name: "coupons");

            migrationBuilder.DropTable(
                name: "passengers");

            migrationBuilder.DropTable(
                name: "tours");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "locations");
        }
    }
}
