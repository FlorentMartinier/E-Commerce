using sansNom.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace sansNom.Controllers
{
    public class HomeController : Controller
    {
        // notre model de base de données
        private Models.BDDEntities4 db = new Models.BDDEntities4();

        // affiche la page d'accueil
        public ActionResult Index(String categorieArticle)
        {
            // on récupère tous les articles en base de données
            var Items = db.Articles;

            // trie des articles suivant leur categorie
            if (categorieArticle == null)
            {
                return View(Items);
            }

            // affichage de tous les articles
            else
            {
                var listItem = new List<Article>();
                foreach(var item in Items){
                    if (item.categorie.Equals(categorieArticle)){
                        listItem.Add(item);
                    }
                }
                return View(listItem);
            }
        }

        // affiche le formulaire de creation de compte
        public ActionResult createAccount()
        {
            return View();
        }

        // formulaire de creation d'articld
        public ActionResult createArticle()
        {
            return View();
        }

        // creation d'un client en base de données
        [HttpPost]
        public ActionResult FormUser(String nomUser, String prenomUser, String mailUser, String passwordUser)
        {
            // creation du client
            Client client = new Client
            {
                nom = nomUser,
                prenom = prenomUser,
                mail = mailUser,
                password = passwordUser
            };
            // on l'ajoute en bdd
            db.Clients.Add(client);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // creation d'un article en base de données
        [HttpPost]
        public ActionResult FormArticle(String titreArticle, String descriptionArticle, String prixArticle, String quantiteArticle, String categorieArticle, HttpPostedFileBase photoArticle)
        {
            // on verifie que l'article soit en stock
            String enStockArticle = "0";
            if (!quantiteArticle.Equals("0"))
            {
                enStockArticle = "1";
            }
                string pic = System.IO.Path.GetFileName(photoArticle.FileName);
                string path = System.IO.Path.Combine(Server.MapPath("/Content/images"), pic);

                // file is uploaded
                photoArticle.SaveAs(path);

                // save the image path path to the database or you can send image 
                // directly to database
                // in-case if you want to store byte[] ie. for DB
                using (MemoryStream ms = new MemoryStream())
                {
                    photoArticle.InputStream.CopyTo(ms);
                    byte[] array = ms.GetBuffer();
                }

            // creation de l'article
                Article article = new Article
                {
                    titre = titreArticle,
                    description = descriptionArticle,
                    prix = float.Parse(prixArticle),
                    quantite = Int32.Parse(quantiteArticle),
                    enStock = Int32.Parse(enStockArticle),
                    categorie = categorieArticle,
                    photo = "/Content/images/" + pic
                };
               
           
            
            // after successfully uploading redirect the user
            db.Articles.Add(article);
            try
            {
                db.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                // Retrieve the error messages as a list of strings.
                var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);

                // Join the list to a single string.
                var fullErrorMessage = string.Join("; ", errorMessages);

                // Combine the original exception message with the new one.
                var exceptionMessage = string.Concat(ex.Message, " The validation errors are: ", fullErrorMessage);

                // Throw a new DbEntityValidationException with the improved exception message.
                throw new DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
            }
            return RedirectToAction("Index");
        }

        // authentification d'un client
        [HttpPost]
        public ActionResult auth(String mailUser, String passwordUser)
        {
            // on récupere le client dont le mail est associé au mail indiqué
            Client client = db.Clients.FirstOrDefault(x => x.mail == mailUser);
            Session["mySession"] = null;
            Session["estAdmin"] = null;
            if (client != null)
            {
                // si le password indiqué est egal au password en bdd, le client se connecte
                if (client.password.Equals(passwordUser))
                {
                    Session["mySession"] = client.Id;
                    Session["nomClient"] = client.prenom;
                    if (client.estAdmin == 1)
                    {
                        Session["estAdmin"] = "true";
                    }
                }
            }

            return RedirectToAction("Index");
        }

        // suppression d'un article
        public ActionResult supprimerArticle(int id)
        {
            var items = db.Paniers;
            Panier panier;

            // on trouve l'article a supprimer
            Article article= db.Articles.FirstOrDefault(x => x.IdArticle == id);
            db.Articles.Remove(article);
            foreach (var item in items)
            {
                // on supprime tous les articles associes dans les paniers
                if (article.IdArticle == item.idArticle)
                {
                    panier = db.Paniers.FirstOrDefault(x => x.idArticle == article.IdArticle);
                    db.Paniers.Remove(panier);
                }
            }
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // details d'un article
        public ActionResult articlePreview(int id)
        {
            Article article = db.Articles.FirstOrDefault(x => x.IdArticle == id);
            return View(article);
        }

        // ajout d'un article dans le panier
        public ActionResult ajouterPanier(int idArticlePanier, int idClientPanier)
        {
            Panier panier = new Panier { 
                idArticle = idArticlePanier,
                idClient = idClientPanier
            };
            db.Paniers.Add(panier);
            db.Articles.Find(idArticlePanier).quantite -= 1;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
        
        // affichage du panier
        public ActionResult panier()
        {
            Article article;
            // chaque panier possède des articles associés
            Dictionary<Panier, Article> map = new Dictionary<Panier,Article>();
            var items = db.Paniers;
            foreach (var item in items)
            {
                // on récupère tous les articles du client connecté
                if (item.idClient.Equals(Session["mySession"]))
                {
                    article = db.Articles.FirstOrDefault(x => x.IdArticle == item.idArticle);
                    map.Add(item, article);
                }
            }
            return View(map);
        }

        // deconnexion du client
        public ActionResult deconnexion()
        {
            Session["mySession"] = null;
            Session["estAdmin"] = null;
            Session["nomClient"] = null;
            return RedirectToAction("Index");
        }

        // formulaire de modification d'article
        public ActionResult modifierArticle(int id)
        {
            Article article = (db.Articles.FirstOrDefault(x => x.IdArticle == id));

            return View(article);
        }

        // on modifie l'article en base de données
        [HttpPost]
        public ActionResult FormModifierArticle(int idArticle, String titreArticle, String descriptionArticle, String prixArticle, String quantiteArticle, String categorieArticle)
        {
            Article article = (db.Articles.FirstOrDefault(x => x.IdArticle == idArticle));

            db.Articles.Find(idArticle).titre = titreArticle;
            db.Articles.Find(idArticle).description = descriptionArticle;
            db.Articles.Find(idArticle).prix = Int32.Parse(prixArticle);
            db.Articles.Find(idArticle).quantite= Int32.Parse(quantiteArticle);
            db.Articles.Find(idArticle).categorie= categorieArticle;

            db.SaveChanges();
            return RedirectToAction("Index");
        }

        // suppression d'un article dans le panier
        public ActionResult supprimerArticlePanier(int idPanier)
        {
            Panier panier = db.Paniers.FirstOrDefault(x => x.id == idPanier);
            Article article = db.Articles.FirstOrDefault(x => x.IdArticle == panier.idArticle);
            db.Paniers.Remove(panier);
            db.Articles.Find(article.IdArticle).quantite += 1;
            db.SaveChanges();
            return RedirectToAction("panier");
        }

        // validation de la commande
        public ActionResult validerCommande(double prix)
        {
            var items = db.Paniers;
            CommandeArticle lien;
            // on créé la commande a rajouter en base de données
            Commande commande = new Commande
            {
                idClient = Int32.Parse(Session["mySession"].ToString()),
                livraisonAdr = "une adresse",
                etat = "en cours",
                dateComande = DateTime.Now
            };

            //on la rajoute
            db.Commandes.Add(commande);
            db.SaveChanges();

            // on supprime tous les articles dans le panier du client actuel
            foreach (var item in items)
            {
                if (item.idClient.ToString().Equals(Session["mySession"].ToString()))
                {
                    // on rajoute tous les articles du panier dans la base de donnees CommandeArticle
                    lien = new CommandeArticle
                    {
                        idArticle = item.idArticle,
                        idCommande = commande.IdCommande
                    };
                    db.CommandeArticles.Add(lien);
                    db.Paniers.Remove(item);
                }
            }
            db.SaveChanges();
            return View(prix);
        }

        // affichage des infos personnelles
        public ActionResult informationsPersonnelles(String idClient)
        {
            Client client = db.Clients.FirstOrDefault(x => x.Id.ToString().Equals(idClient));
            return View(client);
        }

        //formulaire modification des informations personnelles
        public ActionResult modifierInfosPersonnelles(int idClient)
        {
            Client client = db.Clients.FirstOrDefault(x => x.Id == idClient);
            return View(client);
        }

        // modification des infos personnelles en base de données
        public ActionResult FormModifierInfos(int idClientModif, String nomClient, String prenomClient, String mailClient)
        {
            db.Clients.Find(idClientModif).nom = nomClient;
            db.Clients.Find(idClientModif).prenom = prenomClient;
            db.Clients.Find(idClientModif).mail = mailClient;

            db.SaveChanges();
            return RedirectToAction("informationsPersonnelles", new { idClient = idClientModif });
        }
        
        // commandes passées
        public ActionResult mesCommandes(String id)
        {
            List<Commande> listCommande = new List<Commande>();
            var items = db.Commandes;
            foreach (var item in items)
            {
                // on récupère les commandes du client connecté
                if (item.idClient == Int32.Parse(id))
                {
                    listCommande.Add(item);
                }
            }
            return View(listCommande);
        }

        // détail de la commande
        public ActionResult detailCommande(int idCommande)
        {
            List<Article> listArticle = new List<Article>();
            Article article;
            var items = db.CommandeArticles;

            // on recupere tous les articles dans la commande dont l'id est idCommande
            foreach (var item in items)
            {
                if (item.idCommande == idCommande)
                {
                    article = db.Articles.FirstOrDefault(x => x.IdArticle == item.idArticle);
                    listArticle.Add(article);
                }
            }
            return View(listArticle);
        }

    }
}
