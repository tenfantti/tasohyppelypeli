using Jypeli;
using Jypeli.Controls;
using Jypeli.Widgets;
using System.Collections.Generic;

/// @author Janita Sallanko
/// @version 1.12.2015
/// <summary>
/// ElmoPeli, jossa Elmo niminen kani yrittää etsiä takaisin kotia. Salaateista Elmo saa lisäpisteitä, mutta hampurilaiset tappavat Elmon. Pelissä on kolme tasoa.
/// </summary>

public class ElmoPeli : PhysicsGame
{
    const double nopeus = 200;
    const double hyppyNopeus = 750;
    const int RUUDUN_KOKO = 40;
    const int RAJA = 3;
    private int kenttaNro = 1;
    private EasyHighScore topLista = new EasyHighScore();
    private DoubleMeter laskevaLaskuri;
    private Timer aikaLaskuri;
    private IntMeter pisteLaskuri;
    private List<Label> valikonKohdat;

    private PlatformCharacter pelaaja1;
    private Image pelaajanKuva = LoadImage("Elmo");
    private Image salaattiKuva = LoadImage("salaatti");
    private Image taustaKuva = LoadImage("taustakuva");
    private Image kotiKuva = LoadImage("koti");
    private Image burgeriKuva = LoadImage("burgeri");

    private SoundEffect maaliAani = LoadSoundEffect("maali");

    public override void Begin()
    {
        MediaPlayer.Play("ElmoMusiikki"); // Ancient Hero - Royality Free Music
        MediaPlayer.IsRepeating = true;
        Valikko();
    }

    /// <summary>
    /// Lisätään päävalikko.
    /// </summary>
    private void Valikko()
    {
        ClearAll();
        valikonKohdat = new List<Label>();

        Label kohta1 = new Label("Aloita peli");
        kohta1.Position = new Vector(0, 40);
        valikonKohdat.Add(kohta1);
        Label kohta2 = new Label("Parhaat pisteet");
        kohta2.Position = new Vector(0, 0);
        valikonKohdat.Add(kohta2);

        Label kohta3 = new Label("Lopeta peli");
        kohta3.Position = new Vector(0, -40);
        valikonKohdat.Add(kohta3);

        foreach (Label valikonKohta in valikonKohdat)
        {
            Add(valikonKohta);
        }

        Mouse.ListenOn(kohta1, MouseButton.Left, ButtonState.Pressed, SeuraavaKentta, null);
        Mouse.ListenOn(kohta2, MouseButton.Left, ButtonState.Pressed, topLista.Show, null);
        Mouse.ListenOn(kohta3, MouseButton.Left, ButtonState.Pressed, Exit, null);
        Mouse.ListenMovement(1.0, ValikossaLiikkuminen, null);
        Mouse.IsCursorVisible = true;
    }

    /// <summary>
    /// Näyttää valinnan päävalikossa punaisella.
    /// </summary>
    /// <param name="hiirenTila"></param>
    private void ValikossaLiikkuminen(AnalogState hiirenTila)
    {
        foreach (Label kohta in valikonKohdat)
        {
            if (Mouse.IsCursorOn(kohta))
            {
                kohta.TextColor = Color.BloodRed;
            }
            else
            {
                kohta.TextColor = Color.Black;
            }
        }
    }

    /// <summary>
    /// Valikko, joka kysyy jos pelaaja haluaa pelata uudestaan tai lopettaa.
    /// </summary>
    /// <param name="valinta">pelaajan valitsema vaihtoehto</param>
    private void UudestaanValikko(int valinta)
    {
        switch (valinta)
        {
            case 0:
                SeuraavaKentta();
                break;
            case 1:
                Exit();
                break;
        }
    }

    /// <summary>
    /// Aloittaa ensimmäisestä kentästä ja siirtää pelaajan seuraavalle kentälle kun vanha on käyty läpi.
    /// </summary>
    private void SeuraavaKentta()
    {
        ClearAll();
        string[] kentat = new string[] { "kentta1", "kentta2", "kentta3" };
        if (kenttaNro <= RAJA) LuoKentta(kentat[kenttaNro - 1]);
        if (kenttaNro > RAJA) UusiPeli();
        LisaaNappaimet();
        Gravity = new Vector(0, -1000);
        Camera.Follow(pelaaja1);
        Camera.ZoomFactor = 1.2;
        Camera.StayInLevel = true;
        LuoAikaLaskuri();
        LuoPisteLaskuri();
    }

    /// <summary>
    /// Valikko, joka kysyy pelaajalta jos haluaa pelata peliä uudelleen
    /// </summary>
    private void UusiPeli()
    {
        MultiSelectWindow valikko = new MultiSelectWindow("Voitit, jee! Haluatko pelata uudestaan? Paina Enter valitaksesi vaihtoehdon",
"Uudestaan!", "En halua");
        valikko.ItemSelected += UudestaanValikko;
        Add(valikko);
    }

    /// <summary>
    /// Luo kentän peliin.
    /// </summary>
    /// <param name="kentanNimi">nimi kentälle</param>
    private void LuoKentta(string kentanNimi)
    {
        TileMap kentta = TileMap.FromLevelAsset(kentanNimi);
        kentta.SetTileMethod('#', LisaaTaso);
        kentta.SetTileMethod('*', LisaaSalaatti);
        kentta.SetTileMethod('P', LisaaPelaaja);
        kentta.SetTileMethod('K', LisaaKoti);
        kentta.SetTileMethod('B', LisaaBurgeri);
        kentta.Execute(RUUDUN_KOKO, RUUDUN_KOKO);
        Level.CreateBorders();
        Level.Background.CreateGradient(Color.White, Color.Black);
        Level.Background.Image = taustaKuva;
    }

    /// <summary>
    /// Lisätään taso(t) peliin
    /// </summary>
    /// <param name="paikka">mihin taso sijoitetaan</param>
    /// <param name="leveys">millä leveydellä taso sijaitsee</param>
    /// <param name="korkeus">millä korkeudella taso sijaitsee</param>
    public void LisaaTaso(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject(leveys, korkeus);
        taso.Position = paikka;
        taso.Color = Color.Green;
        Add(taso);
    }

     /// <summary>
     /// Lisätään salaatti(a) peliin
     /// </summary>
     /// <param name="paikka">mihin salaatti sijoitetaan</param>
     /// <param name="leveys">millä leveydellä salaatti sijaitsee</param>
     /// <param name="korkeus">millä korkeudella salaatti sijaitsee</param>
    private void LisaaSalaatti(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject salaatti = PhysicsObject.CreateStaticObject(leveys, korkeus);
        salaatti.IgnoresCollisionResponse = true;
        salaatti.Position = paikka;
        salaatti.Image = salaattiKuva;
        salaatti.Tag = "salaatti";
        Add(salaatti);
    }

    /// <summary>
    /// Lisätään koti (= maali) peliin.
    /// </summary>
    /// <param name="paikka">mihin koti sijoitetaan</param>
    /// <param name="leveys">millä leveydellä koti sijaitsee</param>
    /// <param name="korkeus">millä korkeudella koti sijaitsee</param>
    private void LisaaKoti(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject koti = PhysicsObject.CreateStaticObject(leveys, korkeus);
        koti.IgnoresCollisionResponse = true;
        koti.Position = paikka;
        koti.Image = kotiKuva;
        koti.Tag = "koti";
        Add(koti);
    }

    /// <summary>
    /// Lisätään hampurilainen peliin.
    /// </summary>
    /// <param name="paikka">mihin hampurilainen sijoitetaan</param>
    /// <param name="leveys">millä leveydellä hampurilainen sijaitsee</param>
    /// <param name="korkeus">millä korkeudella hampurilainen sijaitsee</param>
    private void LisaaBurgeri(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject burgeri = PhysicsObject.CreateStaticObject(leveys, korkeus);
        burgeri.IgnoresCollisionResponse = true;
        burgeri.Position = paikka;
        burgeri.Image = burgeriKuva;
        burgeri.Tag = "burgeri";
        Add(burgeri);
    }

    /// <summary>
    /// Lisätään Elmo peliin.
    /// </summary>
    /// <param name="paikka">mihin kohtaan Elmo sijoitetaan</param>
    /// <param name="leveys">millä leveydellä Elmo sijaitsee</param>
    /// <param name="korkeus">millä korkeudella Elmo sijaitsee</param>
    private void LisaaPelaaja(Vector paikka, double leveys, double korkeus)
    {
        pelaaja1 = new PlatformCharacter(leveys, korkeus);
        pelaaja1.Position = paikka;
        pelaaja1.Mass = 4.0;
        pelaaja1.Image = pelaajanKuva;
        AddCollisionHandler(pelaaja1, "salaatti", TormaaSalaattiin);
        AddCollisionHandler(pelaaja1, "koti", TormaaKotiin);
        AddCollisionHandler(pelaaja1, "burgeri", TormaaBurgeriin);
        Add(pelaaja1);
    }
    
    /// <summary>
    /// Määrittely näppäimistölle.
    /// </summary>
    public void LisaaNappaimet()
    {
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä ohjeet");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");

        Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja1, -nopeus);
        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Liikkuu vasemmalle", pelaaja1, nopeus);
        Keyboard.Listen(Key.Up, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", pelaaja1, hyppyNopeus);

        ControllerOne.Listen(Button.Back, ButtonState.Pressed, Exit, "Poistu pelistä");

        ControllerOne.Listen(Button.DPadLeft, ButtonState.Down, Liikuta, "Pelaaja liikkuu vasemmalle", pelaaja1, -nopeus);
        ControllerOne.Listen(Button.DPadRight, ButtonState.Down, Liikuta, "Pelaaja liikkuu oikealle", pelaaja1, nopeus);
        ControllerOne.Listen(Button.A, ButtonState.Pressed, Hyppaa, "Pelaaja hyppää", pelaaja1, hyppyNopeus);

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
    }

    /// <summary>
    /// Liikuttaa pelaajaa eteenpäin
    /// </summary>
    /// <param name="hahmo">pelaaja</param>
    /// <param name="nopeus">missä tahdissa halutaan pelaajan liikkuvan</param>
    public static void Liikuta(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Walk(nopeus);
    }

    /// <summary>
    /// Pelaaja pystyy hyppäämään kun painaa nuolta ylös.
    /// </summary>
    /// <param name="hahmo">pelaaja</param>
    /// <param name="nopeus">missä tahdissa halutaan pelaajan ponnahtavan</param>
    public static void Hyppaa(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Jump(nopeus);
    }

    /// <summary>
    /// Pelaajan syödessä salaatin salaatti katoaa ja pelaaja saa lisäpisteen.
    /// </summary>
    /// <param name="hahmo">pelaaja</param>
    /// <param name="salaatti">pelaajan syömä salaatti</param>
    private void TormaaSalaattiin(PhysicsObject hahmo, PhysicsObject salaatti)
    {
        salaatti.Destroy();
        pisteLaskuri.Value += 1;
    }

    /// <summary>
    /// Pelaajan päästessä kotiin tuhoaa kodin ja pelaaja siirtyy seuraavalle kentälle.
    /// </summary>
    /// <param name="hahmo">pelaaja</param>
    /// <param name="koti">koti johon pelaaja pyrkii</param>
    private void TormaaKotiin(PhysicsObject hahmo, PhysicsObject koti)
    {
        koti.Destroy();
        kenttaNro++;
        SeuraavaKentta();
    }

    /// <summary>
    /// Pelaaja kuolee jos se syö hampurilaisen, ja poistaa hampurilaisen pelistä.
    /// </summary>
    /// <param name="hahmo">pelaaja</param>
    /// <param name="burgeri">hampurilainen</param>
    private void TormaaBurgeriin(PhysicsObject hahmo, PhysicsObject burgeri)
    {
        burgeri.Destroy();
        PelaajaKuoli();
    }

    /// <summary>
    /// Pelaajan kuollessa poistaa hahmon ja näyttää parhaat pisteet.
    /// </summary>
    private void PelaajaKuoli()
    {
        pelaaja1.Destroy();
        ParhaatPisteet();
    }

    /// <summary>
    /// Näyttää parhaiden pelaajien listan, ja kysyy jos pelaaja haluaa pelata uudelleen ikkunan sulkiessa.
    /// </summary>
    private void ParhaatPisteet()
    {
        topLista.EnterAndShow(pisteLaskuri.Value);
        topLista.HighScoreWindow.Closed += Uudestaan; 
    }

    /// <summary>
    /// Kysyy jos pelaaja haluaa aloittaa uudestaan.
    /// </summary>
    /// <param name="sender"></param>
    private void Uudestaan(Window sender)
    {
        MultiSelectWindow valikko = new MultiSelectWindow("Annoit Elmon syödä hampurilaisen, hyi! Haluatko pelata uudestaan? Paina Enter valitaksesi vaihtoehdon",
"Höh. Uudestaan!", "Ei kiitos");
        valikko.ItemSelected += UudestaanValikko;
        Add(valikko);
    }

    /// <summary>
    /// Luo pistelaskurin pelille.
    /// </summary>
    private void LuoPisteLaskuri()
    {
        pisteLaskuri = new IntMeter(0);

        Label pisteNaytto = new Label();
        pisteNaytto.X = Screen.Left + 100;
        pisteNaytto.Y = Screen.Top - 100;
        pisteNaytto.TextColor = Color.Black;
        pisteNaytto.Color = Color.White;
        pisteNaytto.Title = "Pisteet";

        pisteNaytto.BindTo(pisteLaskuri);
        Add(pisteNaytto);
    }

    /// <summary>
    /// Luo aikalaskurin pelille.
    /// </summary>
    private void LuoAikaLaskuri()
    {
        laskevaLaskuri = new DoubleMeter(400);

        aikaLaskuri = new Timer();
        aikaLaskuri.Interval = 0.1;
        aikaLaskuri.Timeout += LaskeAlas;
        aikaLaskuri.Start();

        Label aikaNaytto = new Label();
        aikaNaytto.TextColor = Color.BloodRed;
        aikaNaytto.X = Screen.Right - 100;
        aikaNaytto.Y = Screen.Top - 100;
        aikaNaytto.DecimalPlaces = 1;
        aikaNaytto.Title = "Aika";
        aikaNaytto.BindTo(laskevaLaskuri);
        Add(aikaNaytto);
    }

    /// <summary>
    /// Laskee aikalaskurin arvoa alas.
    /// </summary>
    private void LaskeAlas()
    {
        laskevaLaskuri.Value -= 0.1;

        if (laskevaLaskuri.Value <= 0)
        {
            aikaLaskuri.Stop();
            MultiSelectWindow valikko = new MultiSelectWindow("Elmo kuoli nälkään joten hävisit. Haluatko pelata uudestaan? Paina Enter valitaksesi vaihtoehdon",
"Uudestaan!", "En halua");
            valikko.ItemSelected += UudestaanValikko;
            Add(valikko);
        }
    }
}