CREATE TABLE document (
    iddocument VARCHAR(50) PRIMARY KEY,
    nomdocument VARCHAR(200) NOT NULL,
    description TEXT,
    dateajout TIMESTAMP DEFAULT NOW()
);

CREATE TABLE produit_document (
    idproduitdocument VARCHAR(50) PRIMARY KEY,
    idproduit VARCHAR(50) REFERENCES produit(idproduit),
    iddocument VARCHAR(50) REFERENCES document(iddocument)
);

CREATE TABLE produit_document_pays (
    idproduitdocumentpays VARCHAR(50) PRIMARY KEY,
    idproduit VARCHAR(50) REFERENCES produit(idproduit),
    idpays VARCHAR(50) REFERENCES pays(idpays),
    iddocument VARCHAR(50) REFERENCES document(iddocument)
);

CREATE TABLE expedition (
    idexpedition VARCHAR(50) PRIMARY KEY,
    idclient VARCHAR(50) REFERENCES client(idclient),
    paysdestination VARCHAR(100),
    datelivraison DATE,
    datecreation TIMESTAMP DEFAULT NOW(),
    statut VARCHAR(50) DEFAULT 'En préparation'
);

CREATE TABLE expedition_produit (
    idexpeditionproduit VARCHAR(50) PRIMARY KEY,
    idexpedition VARCHAR(50) REFERENCES expedition(idexpedition),
    idproduit VARCHAR(50) REFERENCES produit(idproduit),
    quantite NUMERIC(12,2),
    unite VARCHAR(20),
    prixunitaire NUMERIC(12,2)
);

CREATE TABLE expedition_document (
    idexpeditiondocument VARCHAR(50) PRIMARY KEY,
    idexpedition VARCHAR(50) REFERENCES expedition(idexpedition),
    iddocument VARCHAR(50) REFERENCES document(iddocument),
    statutdocument VARCHAR(50) DEFAULT 'En attente',
    fichierpdf TEXT,
    dateupload TIMESTAMP
);

-------tena izy ----------
🟦 1. Table DOCUMENT
CREATE TABLE document (
    iddocument VARCHAR(50) PRIMARY KEY,
    nomdocument VARCHAR(200) NOT NULL
);

🟦 2. Table PRODUITDOCUMENT
CREATE TABLE produitdocument (
    idproduitdocument VARCHAR(50) PRIMARY KEY,
    idproduit VARCHAR(50) NOT NULL REFERENCES produit(idproduit),
    iddocument VARCHAR(50) NOT NULL REFERENCES document(iddocument)
);

🟦 3. Table PRODUITDOCUMENTPAYS
CREATE TABLE produitdocumentpays (
    idproduitdocumentpays VARCHAR(50) PRIMARY KEY,
    idproduitdocument VARCHAR(50) NOT NULL REFERENCES produitdocument(idproduitdocument),
    idpays VARCHAR(50) NOT NULL REFERENCES pays(idpays)
);

🟦 4. Table EXPEDITION
CREATE TABLE expedition (
    idexpedition VARCHAR(50) PRIMARY KEY,
    idclient VARCHAR(50) NOT NULL REFERENCES client(idclient),
    idpaysdestination VARCHAR(50) NOT NULL REFERENCES pays(idpays),
    datelivraison DATE,
    datecreation TIMESTAMP DEFAULT NOW()
);

🟦 5. Table EXPEDITIONPRODUIT
CREATE TABLE expeditionproduit (
    idexpeditionproduit VARCHAR(50) PRIMARY KEY,
    idexpedition VARCHAR(50) NOT NULL REFERENCES expedition(idexpedition),
    idproduit VARCHAR(50) NOT NULL REFERENCES produit(idproduit),
    quantite NUMERIC(12,2) NOT NULL,
    unite VARCHAR(20) NOT NULL
);

🟦 6. Table EXPEDITIONDOCUMENT
CREATE TABLE expeditiondocument (
    idexpeditiondocument VARCHAR(50) PRIMARY KEY,
    idexpedition VARCHAR(50) NOT NULL REFERENCES expedition(idexpedition),
    iddocument VARCHAR(50) NOT NULL REFERENCES document(iddocument)
);

-- Vanille
INSERT INTO produitdocument VALUES
('PD_001', 'PRO_002', 'DOC_001'),
('PD_002', 'PRO_002', 'DOC_002'),
('PD_003', 'PRO_002', 'DOC_003'),
('PD_004', 'PRO_002', 'DOC_006');

-- Girofle
INSERT INTO produitdocument VALUES
('PD_005', 'PRO_003', 'DOC_003'),
('PD_006', 'PRO_003', 'DOC_004'),
('PD_007', 'PRO_003', 'DOC_006');


INSERT INTO produitdocument VALUES
('PD_008', 'PRO_004', 'DOC_003');

INSERT INTO produitdocument VALUES
('PD_009', 'PRO_001', 'DOC_001'),
('PD_0010', 'PRO_001', 'DOC_002'),
('PD_0011', 'PRO_001', 'DOC_003'),
('PD_0012', 'PRO_001', 'DOC_006');

INSERT INTO produitdocument VALUES
('PD_0013', 'PRO_005', 'DOC_005');


-- Vanille → France
INSERT INTO produitdocumentpays VALUES
('PDP_001', 'PD_001', 'PAY_001'),
('PDP_002', 'PD_002', 'PAY_001'),
('PDP_003', 'PD_003', 'PAY_001');

-- Vanille → USA
INSERT INTO produitdocumentpays VALUES
('PDP_004', 'PD_001', 'PAY_003'),
('PDP_005', 'PD_006', 'PAY_003');

INSERT INTO produitdocumentpays VALUES
('PDP_004', 'PD_001', 'PAY_003'),
('PDP_005', 'PD_006', 'PAY_003');


-- Girofle → France
INSERT INTO produitdocumentpays VALUES
('PDP_006', 'PD_005', 'PAY_001'),
('PDP_007', 'PD_007', 'PAY_001');
---test----
INSERT INTO produitdocument VALUES
('PD_0013', 'PRO_005', 'DOC_005');

-- Canelle -> Grece
INSERT INTO produitdocumentpays VALUES
('PDP_007', 'PD_0013', 'PAY_002');