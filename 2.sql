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
