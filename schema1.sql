CREATE TABLE Client (
    idClient VARCHAR(50),
    nomClient VARCHAR(100)
);

CREATE TABLE Pays (
    idPays VARCHAR(50),
    Pays VARCHAR(100)
);

CREATE TABLE ClientPays (
    idClientPays VARCHAR(50),
    idClient VARCHAR(50),
    idPays VARCHAR(50),
    FOREIGN KEY idClient REFERENCES Client(idClient),
    FOREIGN KEY idPays REFERENCES Pays(idPays)
);

CREATE TABLE Produit(
    idProduit VARCHAR(50),
    nomProduit VARCHAR(100),
    codeHS VARCHAR(100)
);

CREATE TABLE Document (
    idDocument VARCHAR(50),
    nomDocument VARCHAR(100)
)

CREATE TABLE ProduitDocument (
    idProduitDocument VARCHAR(50),
    idProduit VARCHAR(50),
    idDocument VARCHAR(100),
    FOREIGN KEY idProduit REFERENCES Produit(idProduit),
    FOREIGN KEY idDocument REFERENCES Document(idDocument)
);

--- table de liaison entre produit, document et pays 
--ex : vanille a exporter pour japon : doc 1 et2, vanille a exporter pour usa : doc 2, 3 et 4
CREATE TABLE ProduitDocumentPays (
    idProduitDocumentPays VARCHAR(50),
    idProduitDocument VARCHAR(50),
    FOREIGN KEY idProduitDocument REFERENCES ProduitDocument(idProduitDocument)
);

CREATE TABLE Collecteur(
    idCollecteur VARCHAR(50),
    nomCollecteur VARCHAR(100),
)

CREATE TABLE Livraison(
    idLivraison VARCHAR(50),
    idProduit VARCHAR(50),
    FOREIGN KEY idPays

);

CREATE TABLE LivraisonCollecteur(
    idLivraisonCollecteur VARCHAR(50),
    idLIvraison VARCHAR(50),
    idCollecteur VARCHAR(50),
    dateLivraison DATE,
    FOREIGN KEY idLIvraison REFERENCES Livraison(idLivraison),
    FOREIGN KEY idCollecteur REFERENCES Collecteur(idCollecteur)
);
---table de liaison entre produit, collecteur et livraison
--tsy ilaina  
CREATE TABLE LivraisonCollecteurProduit (
    idLivraisonCollecteurProduit VARCHAR
)