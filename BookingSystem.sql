/* =========================
   CREATE DATABASE
========================= */
CREATE DATABASE BookingSystem;
GO
USE BookingSystem;
GO

/* =========================
   ROLE
========================= */
CREATE TABLE Role (
    role_id INT IDENTITY PRIMARY KEY,
    role_name NVARCHAR(50) NOT NULL UNIQUE,
    description NVARCHAR(255),
    created_at DATETIME DEFAULT GETDATE()
);

/* =========================
   PERMISSION
========================= */
CREATE TABLE Permission (
    permission_id INT IDENTITY PRIMARY KEY,
    permission_name NVARCHAR(100) NOT NULL UNIQUE,
    description NVARCHAR(255)
);

/* =========================
   ROLE - PERMISSION
========================= */
CREATE TABLE RolePermission (
    role_id INT NOT NULL,
    permission_id INT NOT NULL,
    PRIMARY KEY (role_id, permission_id),
    FOREIGN KEY (role_id) REFERENCES Role(role_id) ON DELETE CASCADE,
    FOREIGN KEY (permission_id) REFERENCES Permission(permission_id) ON DELETE CASCADE
);

/* =========================
   USER
========================= */
CREATE TABLE [User] (
    user_id INT IDENTITY PRIMARY KEY,
    username NVARCHAR(50) NOT NULL UNIQUE,
    password NVARCHAR(255) NOT NULL,
    email NVARCHAR(100) UNIQUE,
    phone NVARCHAR(20) UNIQUE,
    role_id INT NOT NULL,
    active BIT DEFAULT 1,
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (role_id) REFERENCES Role(role_id)
);

/* =========================
   ROUTE
========================= */
CREATE TABLE Route (
    route_id INT IDENTITY PRIMARY KEY,
    from_location NVARCHAR(100) NOT NULL,
    to_location NVARCHAR(100) NOT NULL,
    is_active BIT DEFAULT 1,
    created_at DATETIME DEFAULT GETDATE(),
    UNIQUE (from_location, to_location)
);

/* =========================
   VEHICLE
========================= */
CREATE TABLE Vehicle (
    vehicle_id INT IDENTITY PRIMARY KEY,
    license_plate NVARCHAR(20) NOT NULL UNIQUE,
    seat_capacity INT NOT NULL,
    vehicle_type NVARCHAR(50),
    is_active BIT DEFAULT 1,
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE()
);

/* =========================
   SEAT
========================= */
CREATE TABLE Seat (
    seat_id INT IDENTITY PRIMARY KEY,
    vehicle_id INT NOT NULL,
    seat_number NVARCHAR(10) NOT NULL,
    seat_type NVARCHAR(20),
    is_active BIT DEFAULT 1,
    FOREIGN KEY (vehicle_id) REFERENCES Vehicle(vehicle_id) ON DELETE CASCADE,
    UNIQUE (vehicle_id, seat_number)
);

/* =========================
   DRIVER
========================= */
CREATE TABLE Driver (
    driver_id INT IDENTITY PRIMARY KEY,
    full_name NVARCHAR(100) NOT NULL,
    phone NVARCHAR(20) NOT NULL UNIQUE,
    license_number NVARCHAR(50) NOT NULL UNIQUE,
    license_type NVARCHAR(20),
    experience_year INT DEFAULT 0,
    is_active BIT DEFAULT 1,
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE()
);

/* =========================
   TRIP
========================= */
CREATE TABLE Trip (
    trip_id INT IDENTITY PRIMARY KEY,
    route_id INT NOT NULL,
    vehicle_id INT NOT NULL,
    driver_id INT NULL,
    departure_time DATETIME NOT NULL,
    arrival_time DATETIME NOT NULL,
    price DECIMAL(12,2) NOT NULL,
    status NVARCHAR(20) DEFAULT 'open',
    created_by INT,
    is_active BIT DEFAULT 1,
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (route_id) REFERENCES Route(route_id),
    FOREIGN KEY (vehicle_id) REFERENCES Vehicle(vehicle_id),
    FOREIGN KEY (driver_id) REFERENCES Driver(driver_id),
    FOREIGN KEY (created_by) REFERENCES [User](user_id),
    CHECK (status IN ('open','full','departed','completed','cancelled'))
);

/* =========================
   PROMOTION
========================= */
CREATE TABLE Promotion (
    promotion_id INT IDENTITY PRIMARY KEY,
    promo_code NVARCHAR(50) NOT NULL UNIQUE,
    description NVARCHAR(255),
    discount_type NVARCHAR(20) NOT NULL,
    discount_value DECIMAL(12,2) NOT NULL,
    min_order_amount DECIMAL(12,2) DEFAULT 0,
    max_discount DECIMAL(12,2),
    start_date DATETIME NOT NULL,
    end_date DATETIME NOT NULL,
    usage_limit INT,
    is_active BIT DEFAULT 1,
    created_at DATETIME DEFAULT GETDATE(),
    CHECK (discount_type IN ('percent','fixed'))
);

/* =========================
   BOOKING
========================= */
CREATE TABLE Booking (
    booking_id INT IDENTITY PRIMARY KEY,
    user_id INT NOT NULL,
    trip_id INT NOT NULL,
    promotion_id INT NULL,
    discount_amount DECIMAL(12,2) DEFAULT 0,
    booking_date DATETIME DEFAULT GETDATE(),
    status NVARCHAR(20) DEFAULT 'pending',
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES [User](user_id),
    FOREIGN KEY (trip_id) REFERENCES Trip(trip_id),
    FOREIGN KEY (promotion_id) REFERENCES Promotion(promotion_id),
    CHECK (status IN ('pending','confirmed','cancelled','expired','refunded'))
);

/* =========================
   TICKET
========================= */
CREATE TABLE Ticket (
    ticket_id INT IDENTITY PRIMARY KEY,
    ticket_code NVARCHAR(50) NOT NULL UNIQUE,
    booking_id INT NOT NULL,
    trip_id INT NOT NULL,
    seat_id INT NOT NULL,
    issued_at DATETIME DEFAULT GETDATE(),
    checkin_at DATETIME NULL,
    status NVARCHAR(20) DEFAULT 'issued',
    FOREIGN KEY (booking_id) REFERENCES Booking(booking_id) ON DELETE CASCADE,
    FOREIGN KEY (trip_id) REFERENCES Trip(trip_id),
    FOREIGN KEY (seat_id) REFERENCES Seat(seat_id),
    CHECK (status IN ('issued','checked_in','cancelled','expired')),
    CONSTRAINT UQ_Ticket_Trip_Seat UNIQUE (trip_id, seat_id)
);

/* =========================
   REVIEW
========================= */
CREATE TABLE Review (
    review_id INT IDENTITY PRIMARY KEY,
    user_id INT NOT NULL,
    trip_id INT NOT NULL,
    driver_id INT NULL,
    rating INT NOT NULL,
    comment NVARCHAR(Max),
    created_at DATETIME DEFAULT GETDATE(),
    CHECK (rating BETWEEN 1 AND 5),
    FOREIGN KEY (user_id) REFERENCES [User](user_id),
    FOREIGN KEY (trip_id) REFERENCES Trip(trip_id),
    FOREIGN KEY (driver_id) REFERENCES Driver(driver_id),
    CONSTRAINT UQ_User_Trip UNIQUE (user_id, trip_id)
);

/* =========================
   OTP TOKEN
========================= */
CREATE TABLE OtpToken (
    otp_id INT IDENTITY PRIMARY KEY,
    user_id INT NOT NULL,
    email NVARCHAR(100) NOT NULL,
    otp_code_hash NVARCHAR(255) NOT NULL,
    otp_type NVARCHAR(30) NOT NULL,
    expires_at DATETIME NOT NULL,
    is_used BIT DEFAULT 0,
    attempt_count INT DEFAULT 0,
    max_attempt INT DEFAULT 5,
    ip_address NVARCHAR(45) NULL,
    user_agent NVARCHAR(255) NULL,
    created_at DATETIME DEFAULT GETDATE(),
    used_at DATETIME NULL,
    CONSTRAINT FK_OtpToken_User FOREIGN KEY (user_id)
        REFERENCES [User](user_id)
        ON DELETE CASCADE,
    CONSTRAINT UQ_OtpToken_User_Email UNIQUE (user_id, otp_type, is_used)
);

/* =========================
   CARGO (HÀNG HÓA)
========================= */
CREATE TABLE Cargo (
    cargo_id INT IDENTITY PRIMARY KEY,
    user_id INT NOT NULL,
    description NVARCHAR(255) NOT NULL,
    weight DECIMAL(12,2) NOT NULL,
    volume DECIMAL(12,2),
    pickup_location NVARCHAR(100) NOT NULL,
    dropoff_location NVARCHAR(100) NOT NULL,
    cargo_type NVARCHAR(50),
    is_active BIT DEFAULT 1,
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES [User](user_id)
);

/* =========================
   CARGO TRIP
========================= */
CREATE TABLE CargoTrip (
    cargo_trip_id INT IDENTITY PRIMARY KEY,
    cargo_id INT NOT NULL,
    trip_id INT NOT NULL,
    status NVARCHAR(20) DEFAULT 'pending',
    assigned_at DATETIME DEFAULT GETDATE(),
    delivered_at DATETIME NULL,
    FOREIGN KEY (cargo_id) REFERENCES Cargo(cargo_id) ON DELETE CASCADE,
    FOREIGN KEY (trip_id) REFERENCES Trip(trip_id),
    CHECK (status IN ('pending','in_transit','delivered','cancelled'))
);

/* =========================
   BILL CHUNG (VÉ & HÀNG)
========================= */
CREATE TABLE Bill (
    bill_id INT IDENTITY PRIMARY KEY,
    user_id INT NOT NULL,
    bill_type NVARCHAR(20) NOT NULL,       -- passenger, cargo
    related_id INT NOT NULL,                -- booking_id hoặc cargo_id tùy bill_type
    total_amount DECIMAL(12,2) NOT NULL,
    discount_amount DECIMAL(12,2) DEFAULT 0,
    final_amount DECIMAL(12,2) NOT NULL,
    status NVARCHAR(20) DEFAULT 'pending', -- pending, paid, refunded, cancelled
    bill_date DATETIME DEFAULT GETDATE(),
    created_at DATETIME DEFAULT GETDATE(),
    updated_at DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (user_id) REFERENCES [User](user_id),
    CHECK (bill_type IN ('passenger','cargo')),
    CHECK (status IN ('pending','paid','refunded','cancelled'))
);

/* =========================
   INDEXES
========================= */
CREATE INDEX IDX_User_Username ON [User](username);
CREATE INDEX IDX_User_Email ON [User](email);
CREATE INDEX IDX_User_Role ON [User](role_id);

CREATE INDEX IDX_Trip_Search ON Trip(route_id, departure_time, status);
CREATE INDEX IDX_Trip_Vehicle ON Trip(vehicle_id);
CREATE INDEX IDX_Trip_Driver ON Trip(driver_id);

CREATE INDEX IDX_Booking_User_Status ON Booking(user_id, status);
CREATE INDEX IDX_Booking_Trip ON Booking(trip_id);

CREATE INDEX IDX_Ticket_Code ON Ticket(ticket_code);
CREATE INDEX IDX_Ticket_Trip ON Ticket(trip_id);
CREATE INDEX IDX_Ticket_Status ON Ticket(status);

CREATE INDEX IDX_Driver_Phone ON Driver(phone);
CREATE INDEX IDX_Driver_Active ON Driver(is_active);

CREATE INDEX IDX_Promotion_Code ON Promotion(promo_code);
CREATE INDEX IDX_Promotion_Date ON Promotion(start_date, end_date);
CREATE INDEX IDX_Promotion_Active ON Promotion(is_active);

CREATE INDEX IDX_Review_Trip ON Review(trip_id);
CREATE INDEX IDX_Review_Driver ON Review(driver_id);
CREATE INDEX IDX_Review_Rating ON Review(rating);

CREATE INDEX IDX_Cargo_User ON Cargo(user_id);
CREATE INDEX IDX_CargoTrip_Trip ON CargoTrip(trip_id);
CREATE INDEX IDX_Bill_User ON Bill(user_id);
CREATE INDEX IDX_Bill_Type ON Bill(bill_type);

