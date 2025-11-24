CREATE DATABASE madamarket;
\c madamarket;

CREATE TABLE role(
    idRole VARCHAR(50),
    roleName VARCHAR(100)
);

CREATE TABLE user (
    idUser VARCHAR(50) PRIMARY KEY,
    userName VARCHAR(100),
    password VARCHAR(100),
    mail VARCHAR(100),
    role VARCHAR(50)
    idRole VARCHAR(50),
    FOREIGN KEY (idRole) REFERENCES role(idRole)
);

CREATE TABLE produit (
    idProduit VARCHAR(50) PRIMARY KEY,
    nomProduit VARCHAR(100),
    code VARCHAR(100)
);

CREATE TABLE document (
    idDocument VARCHAR(50) PRIMARY KEY,
    nomDocument VARCHAR(100)
);

CREATE TABLE pays(
    idPays VARCHAR(50) PRIMARY KEY,
    nomPays VARCHAR(100)
);

CREATE TABLE client(
    idClient VARCHAR(50) PRIMARY KEY,
    nomClient VARCHAR(100)
);

CREATE TABLE clientPays (
    idClientPays VARCHAR(50) PRIMARY KEY,
    idClient VARCHAR(50),
    idPays VARCHAR(50),
    FOREIGN KEY (idClient) REFERENCES client(idClient),
    FOREIGN KEy (idPays) REFERENCES pays(idPays)
);

CREATE TABLE collecteur(
    idCollecteur VARCHAR(50),
    nomCollecteur VARCHAR(100)
    zone VARCHAR(100)
);

CREATE TABLE collecteurProduit (
    idCollecteurProduit VARCHAR(50),
    idCollecteur VARCHAR(50),
    idProduit VARCHAR(50),
    FOREIGN KEY (idCollecteur) REFERENCES collecteur(idCollecteur),
    FOREIGN KEY (produit) REFERENCES produit(idProduit)
);

/*CREATE TABLE zone(
    idZone VARCHAR(50),
    nomZone VARCHAR(100)
);

CREATE TABLE collecteurProduitZone (
    idCollecteurProduitZone VARCHAR(50),
    idCollecteurProduit VARCHAR(100),
    idZone VARCHAR(100),
    FOREIGN KEY idCollecteurProduit REFERENCES collecteurProduit(idCollecteurProduit),
    FOREIGN KEY zone REFERENCES zone(idZone) 
);
*/
----------
CREATE TABLE collecte(
    idCollecte VARCHAR(50),
    
)

ALTER TABLE client
ADD COLUMN mail VARCHAR(100);
ALTER TABLE

UPDATE client SET mail = 'contact@oasis.com' WHERE idclient = 'CLI_001';
UPDATE client SET mail = 'info@nutella.com' WHERE idclient = 'CLI_002';
UPDATE client SET mail = 'contact@kfc.com' WHERE idclient = 'CLI_003';
UPDATE client SET mail = 'test@example.com' WHERE idclient = 'CLI_004';
UPDATE client SET mail = 'contact@kellogs.com' WHERE idclient = 'CLI_005';
UPDATE client SET mail = 'hafa@example.com' WHERE idclient = 'CLI_006';
UPDATE client SET mail = 'leobe@example.com' WHERE idclient = 'CLI_007';
UPDATE client SET mail = 'nicholas.flamel@example.com' WHERE idclient = 'CLI_008';