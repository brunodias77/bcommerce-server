- EXTENSÕES NECESSÁRIAS
CREATE EXTENSION IF NOT EXISTS pgcrypto;
- - ================================================
-- FUNÇÕES GLOBAIS
-- ================================================
- - Atualiza automaticamente o campo updated_at
CREATE OR REPLACE FUNCTION trigger_set_timestamp()
RETURNS TRIGGER AS $$
BEGIN
NEW.updated_at = CURRENT_TIMESTAMP;
RETURN NEW;
END;
$$ LANGUAGE plpgsql;
- - Gera código de pedido
CREATE OR REPLACE FUNCTION generate_order_code()
RETURNS VARCHAR AS $$
BEGIN
RETURN 'ORD-' || UPPER(SUBSTRING(REPLACE(gen_random_uuid()::text, '-', ''), 1, 10));
END;
$$ LANGUAGE plpgsql VOLATILE;
- - ================================================
-- ENUMS
-- ================================================

CREATE TYPE coupon_type AS ENUM ('general', 'user_specific');
CREATE TYPE order_status_enum AS ENUM ('pending', 'processing', 'shipped', 'delivered', 'canceled', 'returned');
CREATE TYPE payment_method_enum AS ENUM ('credit_card', 'debit_card', 'pix', 'bank_slip');
CREATE TYPE payment_status_enum AS ENUM ('pending', 'approved', 'declined', 'refunded', 'partially_refunded', 'chargeback', 'error');
CREATE TYPE address_type_enum AS ENUM ('shipping', 'billing');
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
'LOGIN_SUCCESS', 'LOGIN_FAILURE', 'PASSWORD_RESET_REQUEST',
'PASSWORD_RESET_SUCCESS', 'ORDER_STATUS_CHANGE', 'PAYMENT_STATUS_CHANGE', 'SYSTEM_ACTION'
);

- - ================================================
-- TABELA: client
-- ================================================
CREATE TABLE client (
client_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
first_name VARCHAR(100) NOT NULL,
last_name VARCHAR(155) NOT NULL,
email VARCHAR(255) NOT NULL UNIQUE,
email_verified_at TIMESTAMPTZ,
phone VARCHAR(20) NOT NULL,
password_hash VARCHAR(255) NOT NULL,
cpf CHAR(11) UNIQUE,
date_of_birth DATE,
newsletter_opt_in BOOLEAN NOT NULL DEFAULT FALSE,
status VARCHAR(20) NOT NULL DEFAULT 'ativo'
CHECK (status IN ('ativo', 'inativo', 'banido')),
created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
deleted_at TIMESTAMPTZ,
version INTEGER NOT NULL DEFAULT 1
);

CREATE TRIGGER set_timestamp_client
BEFORE UPDATE ON client
FOR EACH ROW
EXECUTE FUNCTION trigger_set_timestamp();

CREATE INDEX idx_client_status ON client (status);
CREATE INDEX idx_client_newsletter_opt_in ON client (newsletter_opt_in);
CREATE INDEX idx_client_active_records ON client (client_id) WHERE deleted_at IS NULL;

- - ================================================
-- TABELA: category
-- ================================================
CREATE TABLE category (
category_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
name VARCHAR(100) NOT NULL UNIQUE,
slug VARCHAR(150) NOT NULL UNIQUE,
description TEXT,
parent_category_id UUID REFERENCES category(category_id) ON DELETE SET NULL,
is_active BOOLEAN NOT NULL DEFAULT TRUE,
sort_order INTEGER NOT NULL DEFAULT 0,
created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
deleted_at TIMESTAMPTZ,
version INTEGER NOT NULL DEFAULT 1
);

CREATE TRIGGER set_timestamp_category
BEFORE UPDATE ON category
FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

CREATE INDEX idx_category_parent_category_id ON category (parent_category_id) WHERE parent_category_id IS NOT NULL;
CREATE INDEX idx_category_is_active ON category (is_active);
CREATE INDEX idx_category_active_records ON category (category_id) WHERE deleted_at IS NULL;

- - ================================================
-- TABELA: brand
-- ================================================
CREATE TABLE brand (
brand_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
name VARCHAR(100) NOT NULL UNIQUE,
slug VARCHAR(150) NOT NULL UNIQUE,
description TEXT,
logo_url VARCHAR(255),
is_active BOOLEAN NOT NULL DEFAULT TRUE,
created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
deleted_at TIMESTAMPTZ,
version INTEGER NOT NULL DEFAULT 1
);

CREATE TRIGGER set_timestamp_brand
BEFORE UPDATE ON brand
FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

CREATE INDEX idx_brand_is_active ON brand (is_active);
CREATE INDEX idx_brand_active_records ON brand (brand_id) WHERE deleted_at IS NULL;

- - ================================================
-- TABELA: product
-- ================================================
CREATE TABLE product (
product_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
base_sku VARCHAR(50) NOT NULL UNIQUE,
name VARCHAR(150) NOT NULL,
slug VARCHAR(200) NOT NULL UNIQUE,
description TEXT,
category_id UUID NOT NULL REFERENCES category(category_id) ON DELETE RESTRICT,
brand_id UUID REFERENCES brand(brand_id) ON DELETE SET NULL,
base_price NUMERIC(10,2) NOT NULL CHECK (base_price >= 0),
sale_price NUMERIC(10,2) CHECK (sale_price IS NULL OR sale_price >= 0),
sale_price_start_date TIMESTAMPTZ,
sale_price_end_date TIMESTAMPTZ,
stock_quantity INTEGER NOT NULL DEFAULT 0 CHECK (stock_quantity >= 0),
is_active BOOLEAN NOT NULL DEFAULT TRUE,
weight_kg NUMERIC(6,3) CHECK (weight_kg IS NULL OR weight_kg >= 0),
height_cm INTEGER CHECK (height_cm IS NULL OR height_cm >= 0),
width_cm INTEGER CHECK (width_cm IS NULL OR width_cm >= 0),
depth_cm INTEGER CHECK (depth_cm IS NULL OR depth_cm >= 0),
created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
deleted_at TIMESTAMPTZ,
version INTEGER NOT NULL DEFAULT 1
);

CREATE TRIGGER set_timestamp_product
BEFORE UPDATE ON product
FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

CREATE INDEX idx_product_category_id ON product (category_id);
CREATE INDEX idx_product_brand_id ON product (brand_id) WHERE brand_id IS NOT NULL;
CREATE INDEX idx_product_is_active ON product (is_active);
CREATE INDEX idx_product_active_records ON product (product_id) WHERE deleted_at IS NULL;

- - ================================================
-- TABELA: color
-- ================================================
CREATE TABLE color (
color_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
name VARCHAR(50) NOT NULL UNIQUE,
hex_code CHAR(7) UNIQUE CHECK (hex_code ~ '^#[0-9a-fA-F]{6}$'),
is_active BOOLEAN NOT NULL DEFAULT TRUE,
created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
deleted_at TIMESTAMPTZ,
version INTEGER NOT NULL DEFAULT 1
);

CREATE TRIGGER set_timestamp_color
BEFORE UPDATE ON color FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

- - ================================================
-- TABELA: size
-- ================================================
CREATE TABLE size (
size_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
name VARCHAR(50) NOT NULL UNIQUE,
size_code VARCHAR(20) UNIQUE,
description TEXT,
sort_order INTEGER NOT NULL DEFAULT 0,
is_active BOOLEAN NOT NULL DEFAULT TRUE,
created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
deleted_at TIMESTAMPTZ,
version INTEGER NOT NULL DEFAULT 1
);

CREATE TRIGGER set_timestamp_size
BEFORE UPDATE ON size FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

- - ================================================
-- TABELA: product_variant
-- ================================================
CREATE TABLE product_variant (
product_variant_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
product_id UUID NOT NULL REFERENCES product(product_id) ON DELETE CASCADE,
sku VARCHAR(50) NOT NULL UNIQUE,
color_id UUID REFERENCES color(color_id) ON DELETE RESTRICT,
size_id UUID REFERENCES size(size_id) ON DELETE RESTRICT,
stock_quantity INTEGER NOT NULL DEFAULT 0 CHECK (stock_quantity >= 0),
additional_price NUMERIC(10,2) NOT NULL DEFAULT 0.00,
image_url VARCHAR(255),
is_active BOOLEAN NOT NULL DEFAULT TRUE,
created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
deleted_at TIMESTAMPTZ,
version INTEGER NOT NULL DEFAULT 1,
CONSTRAINT uq_product_variant_attributes UNIQUE (product_id, color_id, size_id)
);

CREATE TRIGGER set_timestamp_product_variant
BEFORE UPDATE ON product_variant FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

- - ================================================
-- TABELA: product_attribute
-- ================================================
CREATE TABLE product_attribute (
product_attribute_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
name VARCHAR(100) NOT NULL UNIQUE,
created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
deleted_at TIMESTAMPTZ,
version INTEGER NOT NULL DEFAULT 1
);

CREATE TRIGGER set_timestamp_product_attribute
BEFORE UPDATE ON product_attribute FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

- - ================================================
-- TABELA: product_attribute_value
-- ================================================
CREATE TABLE product_attribute_value (
product_attribute_value_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
product_id UUID NOT NULL REFERENCES product(product_id) ON DELETE CASCADE,
product_attribute_id UUID NOT NULL REFERENCES product_attribute(product_attribute_id) ON DELETE RESTRICT,
value VARCHAR(255) NOT NULL,
created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
deleted_at TIMESTAMPTZ,
version INTEGER NOT NULL DEFAULT 1,
CONSTRAINT uq_product_attribute_value UNIQUE (product_id, product_attribute_id)
);

CREATE TRIGGER set_timestamp_product_attribute_value
BEFORE UPDATE ON product_attribute_value FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

- - ================================================
-- TABELA: tag
-- ================================================
CREATE TABLE tag (
tag_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
name VARCHAR(75) NOT NULL UNIQUE,
created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
deleted_at TIMESTAMPTZ,
version INTEGER NOT NULL DEFAULT 1
);

CREATE TRIGGER set_timestamp_tag
BEFORE UPDATE ON tag FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

- - ================================================
-- TABELA: product_tag
-- ================================================
CREATE TABLE product_tag (
product_tag_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
product_id UUID NOT NULL REFERENCES product(product_id) ON DELETE CASCADE,
tag_id UUID NOT NULL REFERENCES tag(tag_id) ON DELETE CASCADE,
created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
deleted_at TIMESTAMPTZ,
version INTEGER NOT NULL DEFAULT 1,
CONSTRAINT uq_product_tag UNIQUE (product_id, tag_id)
);

CREATE TRIGGER set_timestamp_product_tag
BEFORE UPDATE ON product_tag FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

- - ================================================
-- TABELA: coupon
-- ================================================
CREATE TABLE coupon (
coupon_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
code VARCHAR(50) NOT NULL UNIQUE,
description TEXT,
discount_percentage NUMERIC(5,2),
discount_amount NUMERIC(10,2),
valid_from TIMESTAMPTZ NOT NULL,
valid_until TIMESTAMPTZ NOT NULL,
max_uses INTEGER,
times_used INTEGER NOT NULL DEFAULT 0,
min_purchase_amount NUMERIC(10,2),
is_active BOOLEAN NOT NULL DEFAULT TRUE,
type coupon_type NOT NULL DEFAULT 'general',
client_id UUID REFERENCES client(client_id) ON DELETE RESTRICT,
created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
deleted_at TIMESTAMPTZ,
version INTEGER NOT NULL DEFAULT 1
);

CREATE TRIGGER set_timestamp_coupon
BEFORE UPDATE ON coupon FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

- - ================================================
-- TABELA: "order"
-- ================================================
CREATE TABLE "order" (
order_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
reference_code VARCHAR(20) UNIQUE NOT NULL DEFAULT generate_order_code(),
client_id UUID NOT NULL REFERENCES client(client_id) ON DELETE RESTRICT,
coupon_id UUID REFERENCES coupon(coupon_id) ON DELETE SET NULL,
status order_status_enum NOT NULL DEFAULT 'pending',
items_total_amount NUMERIC(10,2) NOT NULL,
discount_amount NUMERIC(10,2) NOT NULL DEFAULT 0.00,
shipping_amount NUMERIC(10,2) NOT NULL DEFAULT 0.00,
grand_total_amount NUMERIC(12,2) GENERATED ALWAYS AS (items_total_amount - discount_amount + shipping_amount) STORED,
created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
deleted_at TIMESTAMPTZ,
version INTEGER NOT NULL DEFAULT 1
);

CREATE TRIGGER set_timestamp_order
BEFORE UPDATE ON "order"
FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

- - ================================================
-- TABELA: order_item
-- ================================================
CREATE TABLE order_item (
order_item_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
order_id UUID NOT NULL REFERENCES "order"(order_id) ON DELETE CASCADE,
product_id UUID REFERENCES product(product_id) ON DELETE SET NULL,
product_variant_id UUID REFERENCES product_variant(product_variant_id) ON DELETE SET NULL,
item_sku VARCHAR(100) NOT NULL,
item_name VARCHAR(255) NOT NULL,
quantity INTEGER NOT NULL,
unit_price NUMERIC(10,2) NOT NULL,
unit_discount_amount NUMERIC(10,2) NOT NULL DEFAULT 0.00,
line_item_total_amount NUMERIC(12,2) GENERATED ALWAYS AS ((unit_price - unit_discount_amount) * quantity) STORED,
created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
deleted_at TIMESTAMPTZ,
version INTEGER NOT NULL DEFAULT 1
);

CREATE TRIGGER set_timestamp_order_item
BEFORE UPDATE ON order_item
FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

- - ================================================
-- TABELA: payment
-- ================================================
CREATE TABLE payment (
payment_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
order_id UUID NOT NULL REFERENCES "order"(order_id) ON DELETE RESTRICT,
method payment_method_enum NOT NULL,
status payment_status_enum NOT NULL DEFAULT 'pending',
amount NUMERIC(10,2) NOT NULL,
transaction_id VARCHAR(100) UNIQUE,
method_details JSONB,
processed_at TIMESTAMPTZ,
created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
deleted_at TIMESTAMPTZ,
version INTEGER NOT NULL DEFAULT 1
);

CREATE TRIGGER set_timestamp_payment
BEFORE UPDATE ON payment
FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

- - ================================================
-- TABELA: coupon_usage
-- ================================================
CREATE TABLE coupon_usage (
coupon_usage_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
coupon_id UUID NOT NULL REFERENCES coupon(coupon_id) ON DELETE CASCADE,
order_id UUID NOT NULL REFERENCES "order"(order_id) ON DELETE CASCADE,
client_id UUID NOT NULL REFERENCES client(client_id) ON DELETE RESTRICT,
discount_applied_amount NUMERIC(10,2) NOT NULL,
used_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
deleted_at TIMESTAMPTZ,
version INTEGER NOT NULL DEFAULT 1
);

CREATE TRIGGER set_timestamp_coupon_usage
BEFORE UPDATE ON coupon_usage FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

- - ================================================
-- TABELA: address
-- ================================================
CREATE TABLE address (
address_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
client_id UUID NOT NULL REFERENCES client(client_id) ON DELETE CASCADE,
type address_type_enum NOT NULL,
postal_code CHAR(8) NOT NULL,
street VARCHAR(150) NOT NULL,
number VARCHAR(20) NOT NULL,
complement VARCHAR(100),
neighborhood VARCHAR(100) NOT NULL,
city VARCHAR(100) NOT NULL,
state_code CHAR(2) NOT NULL,
country_code CHAR(2) NOT NULL DEFAULT 'BR',
is_default BOOLEAN NOT NULL DEFAULT FALSE,
created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
deleted_at TIMESTAMPTZ,
version INTEGER NOT NULL DEFAULT 1
);

CREATE TRIGGER set_timestamp_address
BEFORE UPDATE ON address FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

- - ================================================
-- TABELA: address_history
-- ================================================
CREATE TABLE address_history (
address_history_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
address_id UUID NOT NULL REFERENCES address(address_id) ON DELETE RESTRICT,
client_id UUID NOT NULL REFERENCES client(client_id) ON DELETE RESTRICT,
address_snapshot JSONB NOT NULL,
created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP
);
- - ================================================
-- TABELA: order_address
-- ================================================
CREATE TABLE order_address (
order_address_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
order_id UUID NOT NULL REFERENCES "order"(order_id) ON DELETE CASCADE,
address_type address_type_enum NOT NULL,
recipient_name VARCHAR(150) NOT NULL,
postal_code CHAR(8) NOT NULL,
street VARCHAR(150) NOT NULL,
number VARCHAR(20) NOT NULL,
complement VARCHAR(100),
neighborhood VARCHAR(100) NOT NULL,
city VARCHAR(100) NOT NULL,
state_code CHAR(2) NOT NULL,
country_code CHAR(2) NOT NULL DEFAULT 'BR',
phone VARCHAR(20),
original_address_id UUID REFERENCES address(address_id) ON DELETE SET NULL,
created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
deleted_at TIMESTAMPTZ,
version INTEGER NOT NULL DEFAULT 1
);

CREATE TRIGGER set_timestamp_order_address
BEFORE UPDATE ON order_address FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

- - ================================================
-- TABELA: cart
-- ================================================
CREATE TABLE cart (
cart_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
client_id UUID UNIQUE NOT NULL REFERENCES client(client_id) ON DELETE CASCADE,
created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
deleted_at TIMESTAMPTZ,
expires_at TIMESTAMPTZ,
version INTEGER NOT NULL DEFAULT 1
);

CREATE TRIGGER set_timestamp_cart
BEFORE UPDATE ON cart FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

- - ================================================
-- TABELA: cart_item
-- ================================================
CREATE TABLE cart_item (
cart_item_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
cart_id UUID NOT NULL REFERENCES cart(cart_id) ON DELETE CASCADE,
product_id UUID REFERENCES product(product_id) ON DELETE CASCADE,
product_variant_id UUID REFERENCES product_variant(product_variant_id) ON DELETE CASCADE,
quantity INTEGER NOT NULL,
created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
deleted_at TIMESTAMPTZ,
version INTEGER NOT NULL DEFAULT 1
);

CREATE TRIGGER set_timestamp_cart_item
BEFORE UPDATE ON cart_item FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

- - ================================================
-- TABELA: favorite
-- ================================================
CREATE TABLE favorite (
favorite_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
client_id UUID NOT NULL REFERENCES client(client_id) ON DELETE CASCADE,
product_id UUID NOT NULL REFERENCES product(product_id) ON DELETE CASCADE,
created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
deleted_at TIMESTAMPTZ,
version INTEGER NOT NULL DEFAULT 1
);

CREATE TRIGGER set_timestamp_favorite
BEFORE UPDATE ON favorite FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

- - ================================================
-- TABELA: review
-- ================================================
CREATE TABLE review (
review_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
client_id UUID REFERENCES client(client_id) ON DELETE SET NULL,
product_id UUID NOT NULL REFERENCES product(product_id) ON DELETE CASCADE,
rating SMALLINT NOT NULL,
comment TEXT,
is_approved BOOLEAN NOT NULL DEFAULT FALSE,
created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
deleted_at TIMESTAMPTZ,
version INTEGER NOT NULL DEFAULT 1
);

CREATE TRIGGER set_timestamp_review
BEFORE UPDATE ON review FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

- - ================================================
-- TABELA: consent
-- ================================================
CREATE TABLE consent (
consent_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
client_id UUID NOT NULL REFERENCES client(client_id) ON DELETE CASCADE,
type consent_type_enum NOT NULL,
consent_subject VARCHAR(100) NOT NULL DEFAULT 'general',
terms_version VARCHAR(30) NOT NULL,
is_granted BOOLEAN NOT NULL,
created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
deleted_at TIMESTAMPTZ,
version INTEGER NOT NULL DEFAULT 1
);

CREATE TRIGGER set_timestamp_consent
BEFORE UPDATE ON consent FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

- - ================================================
-- TABELA: saved_card
-- ================================================
CREATE TABLE saved_card (
saved_card_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
client_id UUID NOT NULL REFERENCES client(client_id) ON DELETE CASCADE,
nickname VARCHAR(50),
last_four_digits CHAR(4) NOT NULL,
brand card_brand_enum NOT NULL,
gateway_token VARCHAR(255) NOT NULL,
expiry_date DATE NOT NULL,
is_active BOOLEAN NOT NULL DEFAULT TRUE,
is_default BOOLEAN NOT NULL DEFAULT FALSE,
created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
updated_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
deleted_at TIMESTAMPTZ,
version INTEGER NOT NULL DEFAULT 1
);

CREATE TRIGGER set_timestamp_saved_card
BEFORE UPDATE ON saved_card FOR EACH ROW EXECUTE FUNCTION trigger_set_timestamp();

- - ================================================
-- TABELA: audit_log
-- ================================================
CREATE TABLE audit_log (
audit_log_id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
table_name VARCHAR(63),
record_id TEXT,
operation_type audit_operation_type_enum NOT NULL,
previous_data JSONB,
new_data JSONB,
change_description TEXT,
user_identifier TEXT,
user_ip_address VARCHAR(45),
logged_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
retention_expires_at TIMESTAMPTZ
);



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






UPDATE client
SET
    email_verified_at = CURRENT_TIMESTAMP,
    updated_at = CURRENT_TIMESTAMP
WHERE
    client_id = 'dcf17a46-1a6d-4883-a141-9e9de29c4310';