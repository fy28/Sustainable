CREATE TABLE role (
    idRole VARCHAR(50) PRIMARY KEY,
    roleName VARCHAR(100) NOT NULL
);

CREATE TABLE "user" (
    idUser VARCHAR(50) PRIMARY KEY,
    userName VARCHAR(100) NOT NULL,
    password VARCHAR(100) NOT NULL,
    mail VARCHAR(100),
    idRole VARCHAR(50) NOT NULL,
    FOREIGN KEY (idRole) REFERENCES role(idRole)
);

CREATE TABLE produit (
    idProduit VARCHAR(50) PRIMARY KEY,
    nomProduit VARCHAR(100) NOT NULL,
    codeHS VARCHAR(100)
);

CREATE TABLE document (
    idDocument VARCHAR(50) PRIMARY KEY,
    nomDocument VARCHAR(200) NOT NULL
);

CREATE TABLE pays (
    idPays VARCHAR(50) PRIMARY KEY,
    nomPays VARCHAR(100) NOT NULL
);

CREATE TABLE client (
    idClient VARCHAR(50) PRIMARY KEY,
    nomClient VARCHAR(100) NOT NULL,
    mail VARCHAR(100)
);

CREATE TABLE clientPays (
    idClientPays VARCHAR(50) PRIMARY KEY,
    idClient VARCHAR(50) NOT NULL,
    idPays VARCHAR(50) NOT NULL,
    FOREIGN KEY (idClient) REFERENCES client(idClient),
    FOREIGN KEY (idPays) REFERENCES pays(idPays)
);

CREATE TABLE zone (
    idZone VARCHAR(50) PRIMARY KEY,
    nomZone VARCHAR(100) NOT NULL
);

CREATE TABLE collecteur (
    idCollecteur VARCHAR(50) PRIMARY KEY,
    nomCollecteur VARCHAR(100) NOT NULL,
    idZone VARCHAR(50) NOT NULL,
    FOREIGN KEY (idZone) REFERENCES zone(idZone)
);

CREATE TABLE collecteurProduit (
    idCollecteurProduit VARCHAR(50) PRIMARY KEY,
    idCollecteur VARCHAR(50) NOT NULL,
    idProduit VARCHAR(50) NOT NULL,
    FOREIGN KEY (idCollecteur) REFERENCES collecteur(idCollecteur),
    FOREIGN KEY (idProduit) REFERENCES produit(idProduit)
);

CREATE TABLE collecte (
    idCollecte VARCHAR(50) PRIMARY KEY,
    idCollecteur VARCHAR(50) NOT NULL,
    dateLivraison DATE,
    dateCollecte DATE,
    FOREIGN KEY (idCollecteur) REFERENCES collecteur(idCollecteur)
);

CREATE TABLE collecte_produit (
    idCollecteProduit VARCHAR(50) PRIMARY KEY,
    idCollecte VARCHAR(50) NOT NULL,
    idProduit VARCHAR(50) NOT NULL,
    quantite NUMERIC(12,2),
    prix NUMERIC(12,2),
    FOREIGN KEY (idCollecte) REFERENCES collecte(idCollecte),
    FOREIGN KEY (idProduit) REFERENCES produit(idProduit)
);

CREATE TABLE produitDocument (
    idProduitDocument VARCHAR(50) PRIMARY KEY,
    idProduit VARCHAR(50) NOT NULL,
    idDocument VARCHAR(50) NOT NULL,
    FOREIGN KEY (idProduit) REFERENCES produit(idProduit),
    FOREIGN KEY (idDocument) REFERENCES document(idDocument)
);

CREATE TABLE produitDocumentPays (
    idProduitDocumentPays VARCHAR(50) PRIMARY KEY,
    idProduitDocument VARCHAR(50) NOT NULL,
    idPays VARCHAR(50) NOT NULL,
    FOREIGN KEY (idProduitDocument) REFERENCES produitDocument(idProduitDocument),
    FOREIGN KEY (idPays) REFERENCES pays(idPays)
);

CREATE TABLE expedition (
    idExpedition VARCHAR(50) PRIMARY KEY,
    idClient VARCHAR(50) NOT NULL,
    idPaysDestination VARCHAR(50) NOT NULL,
    dateLivraison DATE,
    dateCreation TIMESTAMP DEFAULT NOW(),
    FOREIGN KEY (idClient) REFERENCES client(idClient),
    FOREIGN KEY (idPaysDestination) REFERENCES pays(idPays)
);

CREATE TABLE expeditionProduit (
    idExpeditionProduit VARCHAR(50) PRIMARY KEY,
    idExpedition VARCHAR(50) NOT NULL,
    idProduit VARCHAR(50) NOT NULL,
    quantite NUMERIC(12,2) NOT NULL,
    unite VARCHAR(20) NOT NULL,
    FOREIGN KEY (idExpedition) REFERENCES expedition(idExpedition),
    FOREIGN KEY (idProduit) REFERENCES produit(idProduit)
);

CREATE TABLE expeditionDocument (
    idExpeditionDocument VARCHAR(50) PRIMARY KEY,
    idExpedition VARCHAR(50) NOT NULL,
    idDocument VARCHAR(50) NOT NULL,
    FOREIGN KEY (idExpedition) REFERENCES expedition(idExpedition),
    FOREIGN KEY (idDocument) REFERENCES document(idDocument)
);

CREATE TABLE notification (
    idNotification VARCHAR(50) PRIMARY KEY,
    idExpedition VARCHAR(50),
    FOREIGN KEY (idExpedition) REFERENCES expedition(idExpedition)
);

CREATE TABLE niveau (
    idNiveau VARCHAR(50) PRIMARY KEY,
    niveau VARCHAR(100)
);

CREATE TABLE niveauNotification (
    idNiveauNotification VARCHAR(50) PRIMARY KEY,
    idNotification VARCHAR(50),
    idNiveau VARCHAR(50),
    description VARCHAR(100),
    FOREIGN KEY (idNotification) REFERENCES notification(idNotification),
    FOREIGN KEY (idNiveau) REFERENCES niveau(idNiveau)
);

CREATE TABLE statut (
    idStatut VARCHAR(50) PRIMARY KEY,
    nomStatut VARCHAR(100)
);

CREATE TABLE expeditiondocument_statut (
    idExpeditiondocument_statut VARCHAR(50) PRIMARY KEY,
    idStatut VARCHAR(50),
    idExpeditionDocument VARCHAR(50),
    FOREIGN KEY (idStatut) REFERENCES statut(idStatut),
    FOREIGN KEY (idExpeditionDocument) REFERENCES expeditionDocument(idExpeditionDocument)
);


CREATE TABLE paiement(
    idPaiement VARCHAR(50) PRIMARY KEY,
    descri VARCHAR(100),
    datePaiement DATE
);

CREATE TABLE paiementCollecte(
    idPaiementCollecte VARCHAR(50) PRIMARY KEY,
    idPaiement VARCHAR(50),
    idCollecte VARCHAR(50),
    montant NUMERIC(12,2)
    FOREIGN KEY idPaiement REFERENCES paiement(idPaiement),
    FOREIGN KEY idCollecte REFERENCES collecte(idCollecte)
);

CREATE TABLE historique_paiement(
    idHistoriquePaiement VARCHAR(50) PRIMARY KEY,
    idPaiement VARCHAR(50),
    FOREIGN KEY idPaiement REFERENCES paiement(idPaiement)
);

CREATE TABLE notificationHistorique(
    idNotificationHistorique VARCHAR(50) PRIMARY KEY,
    idNotification VARCHAR(50),
    dateNotification DATE,
    FOREIGN KEY idNotification REFERENCES notification(idNotification)
);

