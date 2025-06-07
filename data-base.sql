-- ===================================================================================
-- FUNÇÕES GLOBAIS
-- ===================================================================================

-- Função para atualizar o campo 'updated_at' automaticamente
CREATE OR REPLACE FUNCTION trigger_set_timestamp()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at = CURRENT_TIMESTAMP;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Função exemplo para gerar códigos de pedido (adapte conforme sua necessidade)
CREATE OR REPLACE FUNCTION generate_order_code() 
RETURNS VARCHAR AS $$
BEGIN
    RETURN 'ORD-' || UPPER(SUBSTRING(REPLACE(gen_random_uuid()::text, '-', ''), 1, 10));
END;
$$ LANGUAGE plpgsql VOLATILE;

-- ===================================================================================
-- TIPOS ENUM GLOBAIS
-- ===================================================================================

CREATE TYPE client_status_enum AS ENUM ('ativo', 'inativo', 'banido');

CREATE TYPE coupon_type AS ENUM ('general', 'user_specific');

CREATE TYPE order_status_enum AS ENUM (
    'pending', 'processing', 'shipped', 'delivered', 'canceled', 'returned'
);

CREATE TYPE payment_method_enum AS ENUM (
    'credit_card', 'debit_card', 'pix', 'bank_slip' 
);

CREATE TYPE payment_status_enum AS ENUM (
    'pending', 'approved', 'declined', 'refunded', 'partially_refunded', 'chargeback', 'error'
);

CREATE TYPE address_type_enum AS ENUM (
    'shipping', 'billing'
);

CREATE TYPE consent_type_enum AS ENUM (
    'marketing_email', 'newsletter_subscription', 'terms_of_service', 
    'privacy_policy', 'cookies_essential', 'cookies_analytics', 'cookies_marketing'
);

CREATE TYPE card_brand_enum AS ENUM (
    'visa', 'mastercard', 'amex', 'elo', 'hipercard', 
    'diners_club', 'discover', 'jcb', 'aura', 'other'
);

CREATE TYPE audit_operation_type_enum AS ENUM (
    'INSERT', 'UPDATE', 'DELETE', 
    'LOGIN_SUCCESS', 'LOGIN_FAILURE', 'PASSWORD_RESET_REQUEST', 'PASSWORD_RESET_SUCCESS', 
    'ORDER_STATUS_CHANGE', 'PAYMENT_STATUS_CHANGE', 
    'SYSTEM_ACTION'
);

-- ===================================================================================
-- TABELA: client
-- ===================================================================================
CREATE TABLE client (
    client_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(155) NOT NULL,
    email VARCHAR(255) NOT NULL UNIQUE,
    email_verified_at TIMESTAMPTZ NULL,
    phone VARCHAR(20) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    cpf CHAR(11) UNIQUE NULL,
    date_of_birth DATE NULL,
    newsletter_opt_in BOOLEAN NOT NULL DEFAULT FALSE,
    status client_status_enum NOT NULL DEFAULT 'ativo', -- ATUALIZADO para ENUM
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL
);

CREATE TRIGGER set_timestamp_client
BEFORE UPDATE ON client
FOR EACH ROW
EXECUTE FUNCTION trigger_set_timestamp();

CREATE INDEX idx_client_status ON client (status);
CREATE INDEX idx_client_newsletter_opt_in ON client (newsletter_opt_in);
CREATE INDEX idx_client_active_records ON client (client_id) WHERE deleted_at IS NULL;

-- ===================================================================================
-- TABELA: category
-- ===================================================================================
CREATE TABLE category (
    category_id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    slug VARCHAR(150) NOT NULL UNIQUE,
    description TEXT NULL,
    parent_category_id INTEGER NULL REFERENCES category(category_id) ON DELETE SET NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    sort_order INTEGER NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL
);

CREATE TRIGGER set_timestamp_category
BEFORE UPDATE ON category
FOR EACH ROW
EXECUTE FUNCTION trigger_set_timestamp();

CREATE INDEX idx_category_parent_category_id ON category (parent_category_id) WHERE parent_category_id IS NOT NULL;
CREATE INDEX idx_category_is_active ON category (is_active);
CREATE INDEX idx_category_sort_order ON category (sort_order);
CREATE INDEX idx_category_active_records ON category (category_id) WHERE deleted_at IS NULL;

-- ===================================================================================
-- TABELA: product_attribute
-- ===================================================================================
CREATE TABLE product_attribute (
    product_attribute_id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL
);

CREATE TRIGGER set_timestamp_product_attribute
BEFORE UPDATE ON product_attribute
FOR EACH ROW
EXECUTE FUNCTION trigger_set_timestamp();

CREATE INDEX idx_product_attribute_active_records ON product_attribute (product_attribute_id) WHERE deleted_at IS NULL;

-- ===================================================================================
-- TABELA: tag
-- ===================================================================================
CREATE TABLE tag (
    tag_id SERIAL PRIMARY KEY,
    name VARCHAR(75) NOT NULL UNIQUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL
);

CREATE TRIGGER set_timestamp_tag
BEFORE UPDATE ON tag
FOR EACH ROW
EXECUTE FUNCTION trigger_set_timestamp();

CREATE INDEX idx_tag_active_records ON tag (tag_id) WHERE deleted_at IS NULL;

-- ===================================================================================
-- TABELA: coupon
-- ===================================================================================
CREATE TABLE coupon (
    coupon_id SERIAL PRIMARY KEY,
    code VARCHAR(50) NOT NULL UNIQUE,
    description TEXT NULL,
    discount_percentage NUMERIC(5,2) NULL CHECK (discount_percentage IS NULL OR (discount_percentage > 0 AND discount_percentage <= 100)),
    discount_amount NUMERIC(10,2) NULL CHECK (discount_amount IS NULL OR discount_amount > 0),
    valid_from TIMESTAMPTZ NOT NULL,
    valid_until TIMESTAMPTZ NOT NULL,
    max_uses INTEGER NULL CHECK (max_uses IS NULL OR max_uses > 0),
    times_used INTEGER NOT NULL DEFAULT 0 CHECK (times_used >= 0),
    min_purchase_amount NUMERIC(10,2) NULL CHECK (min_purchase_amount IS NULL OR min_purchase_amount >= 0),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    type coupon_type NOT NULL DEFAULT 'general',
    client_id UUID NULL REFERENCES client(client_id) ON DELETE RESTRICT,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL,
    CONSTRAINT chk_coupon_valid_dates CHECK (valid_until > valid_from),
    CONSTRAINT chk_coupon_usage_limits CHECK (times_used <= max_uses OR max_uses IS NULL),
    CONSTRAINT chk_coupon_type_and_client_consistency CHECK (
        (type = 'user_specific' AND client_id IS NOT NULL) OR
        (type = 'general' AND client_id IS NULL)
    ),
    CONSTRAINT chk_coupon_discount_type CHECK (
        (discount_percentage IS NOT NULL AND discount_amount IS NULL) OR
        (discount_percentage IS NULL AND discount_amount IS NOT NULL) OR
        (discount_percentage IS NULL AND discount_amount IS NULL) 
    )
);

CREATE TRIGGER set_timestamp_coupon
BEFORE UPDATE ON coupon
FOR EACH ROW
EXECUTE FUNCTION trigger_set_timestamp();

CREATE INDEX idx_coupon_client_id ON coupon (client_id) WHERE client_id IS NOT NULL;
CREATE INDEX idx_coupon_is_active_type ON coupon (is_active, type);
CREATE INDEX idx_coupon_valid_until ON coupon (valid_until);
CREATE INDEX idx_coupon_active_records ON coupon (coupon_id) WHERE deleted_at IS NULL;

-- ===================================================================================
-- TABELA: brand
-- ===================================================================================
CREATE TABLE brand (
    brand_id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    slug VARCHAR(150) NOT NULL UNIQUE,
    description TEXT NULL,
    logo_url VARCHAR(255) NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL
);

CREATE TRIGGER set_timestamp_brand
BEFORE UPDATE ON brand
FOR EACH ROW
EXECUTE FUNCTION trigger_set_timestamp();

CREATE INDEX idx_brand_is_active ON brand (is_active);
CREATE INDEX idx_brand_active_records ON brand (brand_id) WHERE deleted_at IS NULL;

-- ===================================================================================
-- TABELA: product
-- ===================================================================================
CREATE TABLE product (
    product_id SERIAL PRIMARY KEY,
    base_sku VARCHAR(50) UNIQUE NOT NULL,
    name VARCHAR(150) NOT NULL,
    slug VARCHAR(200) UNIQUE NOT NULL,
    description TEXT NULL,
    category_id INTEGER NOT NULL REFERENCES category(category_id) ON DELETE RESTRICT,
    brand_id INTEGER NULL REFERENCES brand(brand_id) ON DELETE SET NULL,
    base_price NUMERIC(10,2) NOT NULL CHECK (base_price >= 0),
    sale_price NUMERIC(10,2) NULL CHECK (sale_price IS NULL OR sale_price >= 0),
    sale_price_start_date TIMESTAMPTZ NULL,
    sale_price_end_date TIMESTAMPTZ NULL,
    stock_quantity INTEGER NOT NULL DEFAULT 0 CHECK (stock_quantity >= 0),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    weight_kg NUMERIC(6,3) NULL CHECK (weight_kg IS NULL OR weight_kg >= 0),
    height_cm INTEGER NULL CHECK (height_cm IS NULL OR height_cm >= 0),
    width_cm INTEGER NULL CHECK (width_cm IS NULL OR width_cm >= 0),
    depth_cm INTEGER NULL CHECK (depth_cm IS NULL OR depth_cm >= 0),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL,
    CONSTRAINT chk_product_sale_price_logic CHECK (
        (sale_price IS NULL AND sale_price_start_date IS NULL AND sale_price_end_date IS NULL) OR
        (sale_price IS NOT NULL AND sale_price_start_date IS NOT NULL AND sale_price_end_date IS NOT NULL AND sale_price_end_date > sale_price_start_date)
    ),
    CONSTRAINT chk_product_sale_price_less_than_base_price CHECK (
        sale_price IS NULL OR sale_price < base_price
    )
);

CREATE TRIGGER set_timestamp_product
BEFORE UPDATE ON product
FOR EACH ROW
EXECUTE FUNCTION trigger_set_timestamp();

CREATE INDEX idx_product_category_id ON product (category_id);
CREATE INDEX idx_product_brand_id ON product (brand_id) WHERE brand_id IS NOT NULL;
CREATE INDEX idx_product_is_active ON product (is_active);
CREATE INDEX idx_product_name ON product (name); 
CREATE INDEX idx_product_sale_dates ON product (sale_price_start_date, sale_price_end_date) WHERE sale_price IS NOT NULL AND deleted_at IS NULL;
CREATE INDEX idx_product_active_records ON product (product_id) WHERE deleted_at IS NULL;

-- ===================================================================================
-- TABELA: color
-- ===================================================================================
CREATE TABLE color (
    color_id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    hex_code CHAR(7) NULL UNIQUE CHECK (hex_code IS NULL OR hex_code ~ '^#[0-9a-fA-F]{6}$'),
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL
);

CREATE TRIGGER set_timestamp_color
BEFORE UPDATE ON color
FOR EACH ROW
EXECUTE FUNCTION trigger_set_timestamp();

CREATE INDEX idx_color_is_active ON color (is_active);
CREATE INDEX idx_color_active_records ON color (color_id) WHERE deleted_at IS NULL;

-- ===================================================================================
-- TABELA: size
-- ===================================================================================
CREATE TABLE size (
    size_id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    size_code VARCHAR(20) NULL UNIQUE,
    description TEXT NULL,
    sort_order INTEGER NOT NULL DEFAULT 0,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL
);

CREATE TRIGGER set_timestamp_size
BEFORE UPDATE ON size
FOR EACH ROW
EXECUTE FUNCTION trigger_set_timestamp();

CREATE INDEX idx_size_is_active ON size (is_active);
CREATE INDEX idx_size_sort_order ON size (sort_order);
CREATE INDEX idx_size_active_records ON size (size_id) WHERE deleted_at IS NULL;

-- ===================================================================================
-- TABELA: product_variant
-- ===================================================================================
CREATE TABLE product_variant (
    product_variant_id SERIAL PRIMARY KEY,
    product_id INTEGER NOT NULL REFERENCES product(product_id) ON DELETE CASCADE,
    sku VARCHAR(50) UNIQUE NOT NULL,
    color_id INTEGER NULL REFERENCES color(color_id) ON DELETE RESTRICT,
    size_id INTEGER NULL REFERENCES size(size_id) ON DELETE RESTRICT,
    stock_quantity INTEGER NOT NULL DEFAULT 0 CHECK (stock_quantity >= 0),
    additional_price NUMERIC(10,2) NOT NULL DEFAULT 0.00,
    image_url VARCHAR(255) NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL
    -- A constraint UNIQUE (product_id, color_id, size_id) foi removida para ser substituída por índice único parcial
);

CREATE TRIGGER set_timestamp_product_variant
BEFORE UPDATE ON product_variant
FOR EACH ROW
EXECUTE FUNCTION trigger_set_timestamp();

CREATE UNIQUE INDEX uq_active_variant_attributes ON product_variant (product_id, color_id, size_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_product_variant_product_id ON product_variant (product_id);
CREATE INDEX idx_product_variant_color_id ON product_variant (color_id) WHERE color_id IS NOT NULL;
CREATE INDEX idx_product_variant_size_id ON product_variant (size_id) WHERE size_id IS NOT NULL;
CREATE INDEX idx_product_variant_is_active ON product_variant (is_active);
CREATE INDEX idx_product_variant_active_records ON product_variant (product_variant_id) WHERE deleted_at IS NULL;

-- ===================================================================================
-- TABELA: product_attribute_value
-- ===================================================================================
CREATE TABLE product_attribute_value (
    product_attribute_value_id SERIAL PRIMARY KEY,
    product_id INTEGER NOT NULL REFERENCES product(product_id) ON DELETE CASCADE,
    product_attribute_id INTEGER NOT NULL REFERENCES product_attribute(product_attribute_id) ON DELETE RESTRICT,
    value VARCHAR(255) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL
    -- A constraint UNIQUE (product_id, product_attribute_id) foi removida para ser substituída por índice único parcial
);

CREATE TRIGGER set_timestamp_product_attribute_value
BEFORE UPDATE ON product_attribute_value
FOR EACH ROW
EXECUTE FUNCTION trigger_set_timestamp();

CREATE UNIQUE INDEX uq_active_pav ON product_attribute_value (product_id, product_attribute_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_pav_product_id ON product_attribute_value (product_id);
CREATE INDEX idx_pav_product_attribute_id ON product_attribute_value (product_attribute_id);
CREATE INDEX idx_pav_active_records ON product_attribute_value (product_attribute_value_id) WHERE deleted_at IS NULL;

-- ===================================================================================
-- TABELA: product_tag
-- ===================================================================================
CREATE TABLE product_tag (
    product_tag_id SERIAL PRIMARY KEY,
    product_id INTEGER NOT NULL REFERENCES product(product_id) ON DELETE CASCADE,
    tag_id INTEGER NOT NULL REFERENCES tag(tag_id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL
    -- A constraint UNIQUE (product_id, tag_id) foi removida para ser substituída por índice único parcial
);

CREATE TRIGGER set_timestamp_product_tag
BEFORE UPDATE ON product_tag
FOR EACH ROW
EXECUTE FUNCTION trigger_set_timestamp();

CREATE UNIQUE INDEX uq_active_pt ON product_tag (product_id, tag_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_product_tag_product_id ON product_tag (product_id);
CREATE INDEX idx_product_tag_tag_id ON product_tag (tag_id);
CREATE INDEX idx_product_tag_active_records ON product_tag (product_tag_id) WHERE deleted_at IS NULL;

-- ===================================================================================
-- TABELA: price_history
-- ===================================================================================
CREATE TABLE price_history (
    price_history_id SERIAL PRIMARY KEY,
    product_id INTEGER NULL REFERENCES product(product_id) ON DELETE CASCADE,
    product_variant_id INTEGER NULL REFERENCES product_variant(product_variant_id) ON DELETE CASCADE,
    old_price NUMERIC(10,2) NOT NULL,
    new_price NUMERIC(10,2) NOT NULL,
    change_timestamp TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    reason VARCHAR(255) NULL,
    CONSTRAINT chk_price_history_link CHECK (product_id IS NOT NULL OR product_variant_id IS NOT NULL),
    CONSTRAINT chk_price_history_prices_different CHECK (old_price <> new_price)
);

CREATE INDEX idx_price_history_product_id ON price_history (product_id) WHERE product_id IS NOT NULL;
CREATE INDEX idx_price_history_product_variant_id ON price_history (product_variant_id) WHERE product_variant_id IS NOT NULL;
CREATE INDEX idx_price_history_change_timestamp ON price_history (change_timestamp DESC);

-- ===================================================================================
-- TABELA: "order"
-- ===================================================================================
CREATE TABLE "order" (
    order_id SERIAL PRIMARY KEY,
    reference_code VARCHAR(20) UNIQUE NOT NULL DEFAULT generate_order_code(),
    client_id UUID NOT NULL REFERENCES client(client_id) ON DELETE RESTRICT,
    coupon_id INTEGER NULL REFERENCES coupon(coupon_id) ON DELETE SET NULL,
    status order_status_enum NOT NULL DEFAULT 'pending',
    items_total_amount NUMERIC(10,2) NOT NULL CHECK (items_total_amount >= 0),
    discount_amount NUMERIC(10,2) NOT NULL DEFAULT 0.00 CHECK (discount_amount >= 0),
    shipping_amount NUMERIC(10,2) NOT NULL DEFAULT 0.00 CHECK (shipping_amount >= 0),
    grand_total_amount NUMERIC(12,2) GENERATED ALWAYS AS (items_total_amount - discount_amount + shipping_amount) STORED,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL
);

CREATE TRIGGER set_timestamp_order
BEFORE UPDATE ON "order"
FOR EACH ROW
EXECUTE FUNCTION trigger_set_timestamp();

CREATE INDEX idx_order_client_id ON "order" (client_id);
CREATE INDEX idx_order_status ON "order" (status);
CREATE INDEX idx_order_coupon_id ON "order" (coupon_id) WHERE coupon_id IS NOT NULL;
CREATE INDEX idx_order_created_at ON "order" (created_at DESC);
CREATE INDEX idx_order_active_records ON "order" (order_id) WHERE deleted_at IS NULL;

-- ===================================================================================
-- TABELA: order_item
-- ===================================================================================
CREATE TABLE order_item (
    order_item_id SERIAL PRIMARY KEY,
    order_id INTEGER NOT NULL REFERENCES "order"(order_id) ON DELETE CASCADE,
    product_id INTEGER NULL REFERENCES product(product_id) ON DELETE SET NULL, 
    product_variant_id INTEGER NULL REFERENCES product_variant(product_variant_id) ON DELETE SET NULL,
    item_sku VARCHAR(100) NOT NULL, 
    item_name VARCHAR(255) NOT NULL, 
    quantity INTEGER NOT NULL CHECK (quantity > 0),
    unit_price NUMERIC(10,2) NOT NULL CHECK (unit_price >= 0), 
    unit_discount_amount NUMERIC(10,2) NOT NULL DEFAULT 0.00 CHECK (unit_discount_amount >= 0),
    line_item_total_amount NUMERIC(12,2) GENERATED ALWAYS AS ( (unit_price - unit_discount_amount) * quantity ) STORED,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL,
    CONSTRAINT chk_order_item_link CHECK (product_id IS NOT NULL OR product_variant_id IS NOT NULL OR (item_sku IS NOT NULL AND item_name IS NOT NULL))
);

CREATE TRIGGER set_timestamp_order_item
BEFORE UPDATE ON order_item
FOR EACH ROW
EXECUTE FUNCTION trigger_set_timestamp();

CREATE INDEX idx_order_item_order_id ON order_item (order_id);
CREATE INDEX idx_order_item_product_id ON order_item (product_id) WHERE product_id IS NOT NULL;
CREATE INDEX idx_order_item_product_variant_id ON order_item (product_variant_id) WHERE product_variant_id IS NOT NULL;
CREATE INDEX idx_order_item_item_sku ON order_item (item_sku);
CREATE INDEX idx_order_item_active_records ON order_item (order_item_id) WHERE deleted_at IS NULL;

-- ===================================================================================
-- TABELA: payment
-- ===================================================================================
CREATE TABLE payment (
    payment_id SERIAL PRIMARY KEY,
    order_id INTEGER NOT NULL REFERENCES "order"(order_id) ON DELETE RESTRICT,
    method payment_method_enum NOT NULL,
    status payment_status_enum NOT NULL DEFAULT 'pending',
    amount NUMERIC(10,2) NOT NULL CHECK (amount > 0),
    transaction_id VARCHAR(100) NULL UNIQUE, 
    method_details JSONB NULL,
    processed_at TIMESTAMPTZ NULL,          
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP, 
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL,
    CONSTRAINT chk_payment_processed_details CHECK (
        (status IN ('approved', 'declined', 'error', 'refunded', 'partially_refunded', 'chargeback') AND processed_at IS NOT NULL) OR
        (status NOT IN ('approved', 'declined', 'error', 'refunded', 'partially_refunded', 'chargeback'))
    )
);

CREATE TRIGGER set_timestamp_payment
BEFORE UPDATE ON payment
FOR EACH ROW
EXECUTE FUNCTION trigger_set_timestamp();

CREATE INDEX idx_payment_order_id ON payment (order_id);
CREATE INDEX idx_payment_status ON payment (status);
CREATE INDEX idx_payment_method ON payment (method);
CREATE INDEX idx_payment_transaction_id ON payment (transaction_id) WHERE transaction_id IS NOT NULL;
CREATE INDEX idx_payment_created_at ON payment (created_at);
CREATE INDEX idx_payment_processed_at ON payment (processed_at) WHERE processed_at IS NOT NULL;
CREATE INDEX idx_payment_active_records ON payment (payment_id) WHERE deleted_at IS NULL;

-- ===================================================================================
-- TABELA: coupon_usage
-- ===================================================================================
CREATE TABLE coupon_usage (
    coupon_usage_id SERIAL PRIMARY KEY,
    coupon_id INTEGER NOT NULL REFERENCES coupon(coupon_id) ON DELETE CASCADE,
    order_id INTEGER NOT NULL REFERENCES "order"(order_id) ON DELETE CASCADE, 
    client_id UUID NOT NULL REFERENCES client(client_id) ON DELETE RESTRICT,
    discount_applied_amount NUMERIC(10,2) NOT NULL CHECK (discount_applied_amount > 0),
    used_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL
    -- A constraint UNIQUE (order_id) foi removida para ser substituída por índice único parcial
);

CREATE TRIGGER set_timestamp_coupon_usage
BEFORE UPDATE ON coupon_usage
FOR EACH ROW
EXECUTE FUNCTION trigger_set_timestamp();

CREATE UNIQUE INDEX uq_coupon_usage_order_active ON coupon_usage (order_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_coupon_usage_coupon_id ON coupon_usage (coupon_id);
CREATE INDEX idx_coupon_usage_client_id ON coupon_usage (client_id);
CREATE INDEX idx_coupon_usage_used_at ON coupon_usage (used_at);
CREATE INDEX idx_coupon_usage_active_records ON coupon_usage (coupon_usage_id) WHERE deleted_at IS NULL;

-- ===================================================================================
-- TABELA: address
-- ===================================================================================
CREATE TABLE address (
    address_id SERIAL PRIMARY KEY,
    client_id UUID NOT NULL REFERENCES client(client_id) ON DELETE CASCADE,
    type address_type_enum NOT NULL,
    postal_code CHAR(8) NOT NULL, 
    street VARCHAR(150) NOT NULL, 
    number VARCHAR(20) NOT NULL,  
    complement VARCHAR(100) NULL, 
    neighborhood VARCHAR(100) NOT NULL, 
    city VARCHAR(100) NOT NULL,   
    state_code CHAR(2) NOT NULL,  
    country_code CHAR(2) NOT NULL DEFAULT 'BR',
    is_default BOOLEAN NOT NULL DEFAULT FALSE, 
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL
    -- A constraint UNIQUE (client_id, type, postal_code, street, number, complement, country_code) foi removida para ser substituída por índice único parcial
);

CREATE TRIGGER set_timestamp_address
BEFORE UPDATE ON address
FOR EACH ROW
EXECUTE FUNCTION trigger_set_timestamp();

CREATE UNIQUE INDEX uq_active_address_details ON address (client_id, type, postal_code, street, number, complement, country_code) WHERE deleted_at IS NULL;
CREATE INDEX idx_address_client_id ON address (client_id);
CREATE INDEX idx_address_country_code ON address (country_code);
CREATE UNIQUE INDEX uq_address_default_shipping_per_client 
    ON address (client_id) 
    WHERE type = 'shipping' AND is_default = TRUE AND deleted_at IS NULL;
CREATE UNIQUE INDEX uq_address_default_billing_per_client 
    ON address (client_id) 
    WHERE type = 'billing' AND is_default = TRUE AND deleted_at IS NULL;
CREATE INDEX idx_address_active_records ON address (address_id) WHERE deleted_at IS NULL;

-- ===================================================================================
-- TABELA: address_history
-- ===================================================================================
CREATE TABLE address_history (
    address_history_id SERIAL PRIMARY KEY,
    address_id INTEGER NOT NULL REFERENCES address(address_id) ON DELETE RESTRICT,
    client_id UUID NOT NULL REFERENCES client(client_id) ON DELETE RESTRICT,
    address_snapshot JSONB NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
    -- updated_at e deleted_at omitidos intencionalmente.
);

CREATE INDEX idx_address_history_address_id ON address_history (address_id);
CREATE INDEX idx_address_history_client_id ON address_history (client_id);
CREATE INDEX idx_address_history_created_at ON address_history (created_at DESC);

-- ===================================================================================
-- TABELA: cart
-- ===================================================================================
CREATE TABLE cart (
    cart_id SERIAL PRIMARY KEY,
    client_id UUID UNIQUE NOT NULL REFERENCES client(client_id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL,
    expires_at TIMESTAMPTZ NULL
);

CREATE TRIGGER set_timestamp_cart
BEFORE UPDATE ON cart
FOR EACH ROW
EXECUTE FUNCTION trigger_set_timestamp();

CREATE INDEX idx_cart_expires_at ON cart (expires_at) WHERE expires_at IS NOT NULL AND deleted_at IS NULL;
CREATE INDEX idx_cart_active_records ON cart (cart_id) WHERE deleted_at IS NULL;

-- ===================================================================================
-- TABELA: cart_item
-- ===================================================================================
CREATE TABLE cart_item (
    cart_item_id SERIAL PRIMARY KEY,
    cart_id INTEGER NOT NULL REFERENCES cart(cart_id) ON DELETE CASCADE,
    product_id INTEGER NULL REFERENCES product(product_id) ON DELETE CASCADE, 
    product_variant_id INTEGER NULL REFERENCES product_variant(product_variant_id) ON DELETE CASCADE, 
    quantity INTEGER NOT NULL CHECK (quantity > 0),
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL,
    CONSTRAINT chk_cart_item_link CHECK (product_id IS NOT NULL OR product_variant_id IS NOT NULL)
);

CREATE TRIGGER set_timestamp_cart_item
BEFORE UPDATE ON cart_item
FOR EACH ROW
EXECUTE FUNCTION trigger_set_timestamp();

CREATE INDEX idx_cart_item_cart_id ON cart_item (cart_id);
CREATE INDEX idx_cart_item_product_id ON cart_item (product_id) WHERE product_id IS NOT NULL AND deleted_at IS NULL;
CREATE INDEX idx_cart_item_product_variant_id ON cart_item (product_variant_id) WHERE product_variant_id IS NOT NULL AND deleted_at IS NULL;
CREATE UNIQUE INDEX uq_cart_item_active_product ON cart_item (cart_id, product_id) 
    WHERE product_variant_id IS NULL AND product_id IS NOT NULL AND deleted_at IS NULL;
CREATE UNIQUE INDEX uq_cart_item_active_variant ON cart_item (cart_id, product_variant_id) 
    WHERE product_id IS NULL AND product_variant_id IS NOT NULL AND deleted_at IS NULL;
CREATE INDEX idx_cart_item_active_records ON cart_item (cart_item_id) WHERE deleted_at IS NULL;

-- ===================================================================================
-- TABELA: favorite
-- ===================================================================================
CREATE TABLE favorite (
    favorite_id SERIAL PRIMARY KEY,
    client_id UUID NOT NULL REFERENCES client(client_id) ON DELETE CASCADE,
    product_id INTEGER NOT NULL REFERENCES product(product_id) ON DELETE CASCADE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP, 
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL
    -- A constraint UNIQUE (client_id, product_id) foi removida para ser substituída por índice único parcial
);

CREATE TRIGGER set_timestamp_favorite
BEFORE UPDATE ON favorite
FOR EACH ROW
EXECUTE FUNCTION trigger_set_timestamp();

CREATE UNIQUE INDEX uq_active_favorite ON favorite (client_id, product_id) WHERE deleted_at IS NULL;
CREATE INDEX idx_favorite_client_id ON favorite (client_id);
CREATE INDEX idx_favorite_product_id ON favorite (product_id);
CREATE INDEX idx_favorite_active_records ON favorite (favorite_id) WHERE deleted_at IS NULL;

-- ===================================================================================
-- TABELA: review
-- ===================================================================================
CREATE TABLE review (
    review_id SERIAL PRIMARY KEY,
    client_id UUID NULL REFERENCES client(client_id) ON DELETE SET NULL,
    product_id INTEGER NOT NULL REFERENCES product(product_id) ON DELETE CASCADE,
    rating SMALLINT NOT NULL CHECK (rating BETWEEN 1 AND 5), 
    comment TEXT NULL,
    is_approved BOOLEAN NOT NULL DEFAULT FALSE, 
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP, 
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL
    -- A constraint UNIQUE (client_id, product_id) foi removida para ser substituída por índices únicos parciais
);

CREATE TRIGGER set_timestamp_review
BEFORE UPDATE ON review
FOR EACH ROW
EXECUTE FUNCTION trigger_set_timestamp();

CREATE UNIQUE INDEX uq_review_active_client_product ON review (client_id, product_id) WHERE client_id IS NOT NULL AND deleted_at IS NULL;
CREATE UNIQUE INDEX uq_review_active_anonymous_product ON review (product_id) WHERE client_id IS NULL AND deleted_at IS NULL;
CREATE INDEX idx_review_client_id ON review (client_id) WHERE client_id IS NOT NULL;
CREATE INDEX idx_review_product_id ON review (product_id);
CREATE INDEX idx_review_is_approved ON review (is_approved);
CREATE INDEX idx_review_rating ON review (rating);
CREATE INDEX idx_review_active_records ON review (review_id) WHERE deleted_at IS NULL;

-- ===================================================================================
-- TABELA: consent
-- ===================================================================================
CREATE TABLE consent (
    consent_id SERIAL PRIMARY KEY,
    client_id UUID NOT NULL REFERENCES client(client_id) ON DELETE CASCADE,
    type consent_type_enum NOT NULL,
    consent_subject VARCHAR(100) NOT NULL DEFAULT 'general',
    terms_version VARCHAR(30) NOT NULL,
    is_granted BOOLEAN NOT NULL, 
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL
    -- A constraint UNIQUE (client_id, type, consent_subject, terms_version) foi removida para ser substituída por índice único parcial
);

CREATE TRIGGER set_timestamp_consent
BEFORE UPDATE ON consent
FOR EACH ROW
EXECUTE FUNCTION trigger_set_timestamp();

CREATE UNIQUE INDEX uq_active_consent ON consent (client_id, type, consent_subject, terms_version) WHERE deleted_at IS NULL;
CREATE INDEX idx_consent_client_id ON consent (client_id);
CREATE INDEX idx_consent_type ON consent (type);
CREATE INDEX idx_consent_active_records ON consent (consent_id) WHERE deleted_at IS NULL;

-- ===================================================================================
-- TABELA: saved_card
-- ===================================================================================
CREATE TABLE saved_card (
    saved_card_id SERIAL PRIMARY KEY,
    client_id UUID NOT NULL REFERENCES client(client_id) ON DELETE CASCADE,
    nickname VARCHAR(50) NULL,
    last_four_digits CHAR(4) NOT NULL,
    brand card_brand_enum NOT NULL,
    gateway_token VARCHAR(255) NOT NULL,
    expiry_date DATE NOT NULL,
    is_active BOOLEAN NOT NULL DEFAULT TRUE,
    is_default BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL
    -- A constraint UNIQUE (client_id, gateway_token) foi removida para ser substituída por índice único parcial
);

CREATE TRIGGER set_timestamp_saved_card
BEFORE UPDATE ON saved_card
FOR EACH ROW
EXECUTE FUNCTION trigger_set_timestamp();

CREATE UNIQUE INDEX uq_active_sccgt ON saved_card (client_id, gateway_token) WHERE deleted_at IS NULL;
CREATE INDEX idx_saved_card_client_id ON saved_card (client_id);
CREATE INDEX idx_saved_card_is_active ON saved_card (is_active);
CREATE UNIQUE INDEX uq_saved_card_default_per_client 
    ON saved_card (client_id) 
    WHERE is_default = TRUE AND deleted_at IS NULL;
CREATE INDEX idx_saved_card_active_records ON saved_card (saved_card_id) WHERE deleted_at IS NULL;

-- ===================================================================================
-- TABELA: audit_log
-- ===================================================================================
CREATE TABLE audit_log (
    audit_log_id BIGSERIAL PRIMARY KEY,
    table_name VARCHAR(63) NULL,
    record_id TEXT NULL,
    operation_type audit_operation_type_enum NOT NULL,
    previous_data JSONB NULL,
    new_data JSONB NULL,
    change_description TEXT NULL,
    user_identifier TEXT NULL,
    user_ip_address VARCHAR(45) NULL,
    logged_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    retention_expires_at TIMESTAMPTZ NULL DEFAULT (CURRENT_TIMESTAMP + INTERVAL '2 years')
);

CREATE INDEX idx_audit_log_table_record ON audit_log (table_name, record_id) WHERE table_name IS NOT NULL AND record_id IS NOT NULL;
CREATE INDEX idx_audit_log_operation_type ON audit_log (operation_type);
CREATE INDEX idx_audit_log_user_identifier ON audit_log (user_identifier) WHERE user_identifier IS NOT NULL;
CREATE INDEX idx_audit_log_logged_at ON audit_log (logged_at DESC);
CREATE INDEX idx_audit_log_retention_expires_at ON audit_log (retention_expires_at) WHERE retention_expires_at IS NOT NULL;

-- ===================================================================================
-- TABELA: order_address
-- ===================================================================================
CREATE TABLE order_address (
    order_address_id SERIAL PRIMARY KEY,
    order_id INTEGER NOT NULL REFERENCES "order"(order_id) ON DELETE CASCADE,
    address_type address_type_enum NOT NULL,
    recipient_name VARCHAR(150) NOT NULL,
    postal_code CHAR(8) NOT NULL, 
    street VARCHAR(150) NOT NULL, 
    number VARCHAR(20) NOT NULL,  
    complement VARCHAR(100) NULL, 
    neighborhood VARCHAR(100) NOT NULL, 
    city VARCHAR(100) NOT NULL,   
    state_code CHAR(2) NOT NULL,  
    country_code CHAR(2) NOT NULL DEFAULT 'BR',
    phone VARCHAR(20) NULL,
    original_address_id INTEGER NULL REFERENCES address(address_id) ON DELETE SET NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    deleted_at TIMESTAMPTZ NULL
    -- A constraint UNIQUE (order_id, address_type) foi removida para ser substituída por índice único parcial
);

CREATE TRIGGER set_timestamp_order_address
BEFORE UPDATE ON order_address
FOR EACH ROW
EXECUTE FUNCTION trigger_set_timestamp();

CREATE UNIQUE INDEX uq_active_oa_type ON order_address (order_id, address_type) WHERE deleted_at IS NULL;
CREATE INDEX idx_order_address_order_id ON order_address (order_id);
CREATE INDEX idx_order_address_original_address_id ON order_address (original_address_id) WHERE original_address_id IS NOT NULL;
CREATE INDEX idx_order_address_active_records ON order_address (order_address_id) WHERE deleted_at IS NULL;


-- ================================================
-- TABELA: email_verification_token
-- ================================================
CREATE TABLE email_verification_token (
    token_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    client_id UUID NOT NULL REFERENCES client(client_id) ON DELETE CASCADE,
    token_hash VARCHAR(255) NOT NULL UNIQUE, -- Armazenamos o hash do token, não o token em si
    expires_at TIMESTAMPTZ NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_evt_client_id ON email_verification_token (client_id);