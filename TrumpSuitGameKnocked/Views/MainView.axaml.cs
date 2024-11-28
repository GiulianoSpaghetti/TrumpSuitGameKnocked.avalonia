using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using org.altervista.numerone.framework;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
namespace TrumpSuitGameKnocked.Views;

public partial class MainView : UserControl
{

    private static Giocatore g, cpu, primo, secondo, temp;
    private static Mazzo m;
    private static Carta c, c1, briscola;
    private static Image cartaCpu = new Image();
    private static Image i, i1;
    private static UInt16 puntiUtente = 0, puntiCpu = 0;
    private static UInt128 partite = 0;
    private static bool avvisaTalloneFinito = true, briscolaDaPunti = false, primaUtente = true, stessoSeme = false;
    private static GiocatoreHelperCpu helper;
    private static readonly string folder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CBriscola.Avalonia");
    public static ResourceDictionary d;

    private ElaboratoreCarteBriscola e;
    private Stream asset;
    private Opzioni o;
    private static org.altervista.numerone.framework.briscola.CartaHelper cartaHelper;
    public static MainView Instance=null;
    public MainView()
    {
        InitializeComponent();
        Instance = this;
        d = this.FindResource(CultureInfo.CurrentCulture.TwoLetterISOLanguageName) as ResourceDictionary;
        if (d == null)
            d = this.FindResource("it") as ResourceDictionary;

        o = LeggiOpzioni();
        briscolaDaPunti = o.briscolaDaPunti;

        e = new ElaboratoreCarteBriscola(briscolaDaPunti);
        m = new Mazzo(e);

        m.SetNome(o.nomeMazzo);
        Carta.Inizializza("", m, 40, cartaHelper = new org.altervista.numerone.framework.briscola.CartaHelper(ElaboratoreCarteBriscola.GetCartaBriscola()), d["bastoni"] as string, d["coppe"] as string, d["denari"] as string, d["spade"] as string, d["Fiori"] as string, d["Quadri"] as string, d["Cuori"] as string, d["Picche"] as string, "TrumpSuitGameKnocked");

        asset = AssetLoader.Open(new Uri($"avares://TrumpSuitGameKnocked/Assets/retro_carte_pc.png"));
        cartaCpu.Source = new Bitmap(asset);

        g = new Giocatore(new GiocatoreHelperUtente(), o.NomeUtente, 3);
        switch (o.livello)
        {
            case 1: helper = new GiocatoreHelperCpu0(ElaboratoreCarteBriscola.GetCartaBriscola()); break;
            case 2: helper = new GiocatoreHelperCpu1(ElaboratoreCarteBriscola.GetCartaBriscola()); break;
            default: helper = new GiocatoreHelperCpu2(ElaboratoreCarteBriscola.GetCartaBriscola()); break;

        }
        cpu = new Giocatore(helper, o.NomeCpu, 3);
        avvisaTalloneFinito = o.avvisaTalloneFinito;
        stessoSeme = o.stessoSeme;
        primo = g;
        secondo = cpu;
        briscola = Carta.GetCarta(ElaboratoreCarteBriscola.GetCartaBriscola());
        Image[] img = new Image[3];
        for (UInt16 i = 0; i < 3; i++)
        {
            g.AddCarta(m);
            cpu.AddCarta(m);

        }
        NomeUtente.Content = g.GetNome();
        NomeCpu.Content = cpu.GetNome();
        Utente0.Source = g.GetImmagine(0);
        Utente1.Source = g.GetImmagine(1);
        Utente2.Source = g.GetImmagine(2);
        Cpu0.Source = cartaCpu.Source;
        Cpu1.Source = cartaCpu.Source;
        Cpu2.Source = cartaCpu.Source;
        PuntiCpu.Content = $"{d["PuntiDiPrefisso"]} {cpu.GetNome()} {d["PuntiDiSuffisso"]}: {cpu.GetPunteggio()}";
        PuntiUtente.Content = $"{d["PuntiDiPrefisso"]} {g.GetNome()} {d["PuntiDiSuffisso"]}: {g.GetPunteggio()}";
        NelMazzoRimangono.Content = $"{d["NelMazzoRimangono"]} {m.GetNumeroCarte()} {d["carte"]}";
        CartaBriscola.Content = $"{d["IlSemeDiBriscolaE"]}: {briscola.GetSemeStr()}";
        lbCartaBriscola.Content = $"{d["BriscolaDaPunti"]}";
        lbAvvisaTallone.Content = $"{d["AvvisaTallone"]}";
        opNomeUtente.Content = $"{d["NomeUtente"]}: ";
        opNomeCpu.Content = $"{d["NomeCpu"]}: ";
        InfoApplicazione.Content = $"{d["Applicazione"]}";
        OpzioniApplicazione.Content = $"{d["Applicazione"]}";
        OpzioniInformazioni.Content = $"{d["Informazioni"]}";
        AppInformazioni.Content = $"{d["Informazioni"]}";
        AppOpzioni.Content = $"{d["Opzioni"]}";
        fpOk.Content = $"{d["Si"]}";
        fpCancel.Content = $"{d["No"]}";
        Briscola.Source = briscola.GetImmagine();
        btnGiocata.Content = $"{d["giocataVista"]}";
        lbLivello.Content = $"{d["Livello"]}";
    }
    private Opzioni CaricaOpzioni()
    {
        Opzioni o;

        o = LeggiOpzioni();
        return o;
    }
    private Opzioni LeggiOpzioni()
    {
        Opzioni o;
        if (!System.IO.Path.Exists(folder))
            Directory.CreateDirectory(folder);
        StreamReader file;
        try
        {
            file = new StreamReader(System.IO.Path.Combine(folder, "opzioni.json"));
            o = Newtonsoft.Json.JsonConvert.DeserializeObject<Opzioni>(file.ReadToEnd());
            if (o == null)
                throw new FileNotFoundException();
        }
        catch (FileNotFoundException ex)
        {
            o = new Opzioni();
            o.NomeUtente = "numerone";
            o.NomeCpu = "numerona";
            o.briscolaDaPunti = false;
            o.avvisaTalloneFinito = true;
            o.nomeMazzo = "Napoletano";
            o.livello = 3;
            o.stessoSeme = false;
            SalvaOpzioni(o);
            return o;
        }
        file.Close();
        return o;
    }

    private void SalvaOpzioni(Opzioni o)
    {
        StreamWriter w = new StreamWriter($"{System.IO.Path.Combine(folder, "opzioni.json")}");
        w.Write(Newtonsoft.Json.JsonConvert.SerializeObject(o));
        w.Close();
    }


    private void Gioca_Click(object sender, RoutedEventArgs e)
    {
        c = primo.GetCartaGiocata();
        c1 = secondo.GetCartaGiocata();
        if ((c.CompareTo(c1) > 0 && c.StessoSeme(c1)) || (c1.StessoSeme(briscola) && !c.StessoSeme(briscola)))
        {
            temp = secondo;
            secondo = primo;
            primo = temp;
        }

        primo.AggiornaPunteggio(secondo);
        PuntiCpu.Content = $"{d["PuntiDiPrefisso"]} {cpu.GetNome()} {d["PuntiDiSuffisso"]}: {cpu.GetPunteggio()}";
        PuntiUtente.Content = $"{d["PuntiDiPrefisso"]} {g.GetNome()} {d["PuntiDiSuffisso"]}: {g.GetPunteggio()}";
        if (AggiungiCarte())
        {
            NelMazzoRimangono.Content = $"{d["NelMazzoRimangono"]} {m.GetNumeroCarte()} {d["carte"]}";
            CartaBriscola.Content = $"{d["IlSemeDiBriscolaE"]}: {briscola.GetSemeStr()}";
            if (Briscola.IsVisible)
            {
                switch (m.GetNumeroCarte())
                {
                    case 2: MainWindow.MakeNotification("Mazzo finito", "Il mazzo è finito"); break;
                    case 0:
                        NelMazzoRimangono.IsVisible = false;
                        Briscola.IsVisible = false;
                        break;
                }
            }
            Utente0.Source = g.GetImmagine(0);
            if (cpu.GetNumeroCarte() > 1)
                Utente1.Source = g.GetImmagine(1);
            if (cpu.GetNumeroCarte() > 2)
                Utente2.Source = g.GetImmagine(2);
            i.IsVisible = true;
            i1.IsVisible = true;
            Giocata0.IsVisible = false;
            Giocata1.IsVisible = false;
            if (cpu.GetNumeroCarte() == 2)
            {
                Utente2.IsVisible = false;
                Cpu2.IsVisible = false;
            }
            if (cpu.GetNumeroCarte() == 1)
            {
                Utente1.IsVisible = false;
                Cpu1.IsVisible = false;
            }
            if (primo == cpu)
            {
                i1 = GiocaCpu();
                if (cpu.GetCartaGiocata().StessoSeme(briscola))
                {
                    MainWindow.MakeNotification("La cpu ha giocato briscola", $"La cpu ha giocato il {cpu.GetCartaGiocata().GetValore() + 1} di briscola");

                }
                else if (cpu.GetCartaGiocata().GetPunteggio() > 0)
                {

                    MainWindow.MakeNotification("La cpu ha giocato briscola", $"La cpu ha giocato il {cpu.GetCartaGiocata().GetValore() + 1} di {cpu.GetCartaGiocata().GetSemeStr()}");

                }
            }
        }
        else
        {
            String s;
            puntiUtente += g.GetPunteggio();
            puntiCpu += cpu.GetPunteggio();
            if (puntiUtente == puntiCpu)
                s = $"{d["PartitaPatta"]}";
            else
            {
                if (puntiUtente > puntiCpu)
                    s = $"{d["HaiVinto"]}";
                else
                    s = $"{d["HaiPerso"]}";
                s = $"{s} {d["per"]} {Math.Abs(puntiUtente - puntiCpu)} {d["punti"]}";
            }
            if (partite % 2 == 1)
            {
                fpRisultrato.Content = $"{d["PartitaFinita"]}. {s}. {d["NuovaPartita"]}?";
            }
            else
            {
                fpRisultrato.Content = $"{d["PartitaFinita"]}. {s}. {d["EffettuaSecondaPartita"]}?";
            }
            Applicazione.IsVisible = false;
            FinePartita.IsVisible = true;
            partite++;
        }
        btnGiocata.IsVisible = false;
    }
    private Image GiocaUtente(Image img)
    {
        UInt16 quale = 0;
        Image img1 = Utente0;
        if (img == Utente1)
        {
            quale = 1;
            img1 = Utente1;
        }
        if (img == Utente2)
        {
            quale = 2;
            img1 = Utente2;
        }
        if (primo == g)
            g.Gioca(quale);
        else
            g.Gioca(quale, primo, true);

        Giocata0.IsVisible = true;
        Giocata0.Source = img1.Source;
        img1.IsVisible = false;
        return img1;
    }

    private void OnInfo_Click(object sender, RoutedEventArgs e)
    {
        Applicazione.IsVisible = false;
        GOpzioni.IsVisible = false;
        Info.IsVisible = true;
    }

    private void OnApp_Click(object sender, RoutedEventArgs e)
    {
        GOpzioni.IsVisible = false;
        Info.IsVisible = false;
        Applicazione.IsVisible = true;
    }
    private void OnOpzioni_Click(object sender, RoutedEventArgs e)
    {
        Info.IsVisible = false;
        Applicazione.IsVisible = false;
        GOpzioni.IsVisible = true;
        txtNomeUtente.Text = g.GetNome();
        txtCpu.Text = cpu.GetNome();
        cbCartaBriscola.IsChecked = briscolaDaPunti;
        cbAvvisaTallone.IsChecked = avvisaTalloneFinito;
        cbLivello.SelectedIndex = helper.GetLivello() - 1;

    }

    private void NuovaPartita(bool vecchioStessoSeme)
    {
        if (partite % 2 == 0)
            puntiUtente = puntiCpu = 0;
        bool cartaBriscola = true;
        if (cbCartaBriscola.IsChecked == false)
            cartaBriscola = false;
        e = new ElaboratoreCarteBriscola(cartaBriscola);
        m = new Mazzo(e);
        Carta.SetHelper(cartaHelper = new org.altervista.numerone.framework.briscola.CartaHelper(ElaboratoreCarteBriscola.GetCartaBriscola()));
        m.SetNome(o.nomeMazzo);
        briscola = Carta.GetCarta(ElaboratoreCarteBriscola.GetCartaBriscola());
        g = new Giocatore(new GiocatoreHelperUtente(), g.GetNome(), 3);
        switch (o.livello)
        {
            case 1: helper = new GiocatoreHelperCpu0(ElaboratoreCarteBriscola.GetCartaBriscola()); break;
            case 2: helper = new GiocatoreHelperCpu1(ElaboratoreCarteBriscola.GetCartaBriscola()); break;
            default: helper = new GiocatoreHelperCpu2(ElaboratoreCarteBriscola.GetCartaBriscola()); break;

        }
        cpu = new Giocatore(helper, cpu.GetNome(), 3);
        g = new Giocatore(new GiocatoreHelperUtente(), g.GetNome(), 3);
        for (UInt16 i = 0; i < 3; i++)
        {
            g.AddCarta(m);
            cpu.AddCarta(m);

        }
        Utente0.Source = g.GetImmagine(0);
        Utente0.IsVisible = true;
        Utente1.Source = g.GetImmagine(1);
        Utente1.IsVisible = true;
        Utente2.Source = g.GetImmagine(2);
        Utente2.IsVisible = true;
        Cpu0.Source = cartaCpu.Source;
        Cpu0.IsVisible = true;
        Cpu1.Source = cartaCpu.Source;
        Cpu1.IsVisible = true;
        Cpu2.Source = cartaCpu.Source;
        Cpu2.IsVisible = true;
        Giocata0.IsVisible = false;
        Giocata1.IsVisible = false;
        PuntiCpu.Content = $"{d["PuntiDiPrefisso"]} {cpu.GetNome()} {d["PuntiDiSuffisso"]}: {cpu.GetPunteggio()}";
        PuntiUtente.Content = $"{d["PuntiDiPrefisso"]} {g.GetNome()} {d["PuntiDiSuffisso"]}: {g.GetPunteggio()}";
        NelMazzoRimangono.Content = $"{d["NelMazzoRimangono"]} {m.GetNumeroCarte()} {d["carte"]}";
        NelMazzoRimangono.IsVisible = true;
        CartaBriscola.Content = $"{d["IlSemeDiBriscolaE"]}: {briscola.GetSemeStr()}";
        CartaBriscola.IsVisible = true;
        Briscola.Source = briscola.GetImmagine();
        Briscola.IsVisible = true;
        primaUtente = !primaUtente;
        btnGiocata.IsVisible = false;
        if (primaUtente)
        {
            primo = g;
            secondo = cpu;
        }
        else
        {
            primo = cpu;
            secondo = g;
            i1 = GiocaCpu();
        }
        Briscola.Source = briscola.GetImmagine();

    }
    private void OnOkFp_Click(object sender, RoutedEventArgs evt)
    {
        FinePartita.IsVisible = false;
        NuovaPartita(stessoSeme);
        Applicazione.IsVisible = true;

    }
    private void OnCancelFp_Click(object sender, RoutedEventArgs e)
    {
        fpRisultrato.Content = d["ApplicazioneTerminata"];
        fpOk.IsVisible = false;
        fpCancel.IsVisible = false;
    }

    private Image GiocaCpu()
    {
        UInt16 quale = 0;
        Image img1 = Cpu0;
        if (primo == cpu)
            cpu.Gioca(0);
        else
            cpu.Gioca(0, g, true);
        quale = cpu.GetICartaGiocata();
        if (quale == 1)
            img1 = Cpu1;
        if (quale == 2)
            img1 = Cpu2;
        Giocata1.IsVisible = true;
        Giocata1.Source = cpu.GetCartaGiocata().GetImmagine();
        img1.IsVisible = false;
        return img1;
    }
    private static bool AggiungiCarte()
    {
        try
        {
            primo.AddCarta(m);
            secondo.AddCarta(m);
        }
        catch (IndexOutOfRangeException e)
        {
            return false;
        }
        return true;
    }

    private void Image_Tapped(object Sender, RoutedEventArgs arg)
    {
        if (btnGiocata.IsVisible)
            return;
        Image img = (Image)((Button)Sender).Content;
        try
        {
            i = GiocaUtente(img);

        }
        catch (Exception ex)
        {
            MainWindow.MakeNotification("Mossa non consentita", d["MossaNonConsentitaTesto"] as string);
            return;
        }
        if (secondo == cpu)
            i1 = GiocaCpu();
        btnGiocata.IsVisible = true;
    }
    public void OnOk_Click(Object source, RoutedEventArgs evt)
    {
        bool vecchioStessoSeme = o.stessoSeme;
        g.SetNome(txtNomeUtente.Text);
        cpu.SetNome(txtCpu.Text);
        if (cbCartaBriscola.IsChecked == false)
            briscolaDaPunti = false;
        else
            briscolaDaPunti = true;
        if (cbAvvisaTallone.IsChecked == false)
            avvisaTalloneFinito = false;
        else
            avvisaTalloneFinito = true;
        NomeUtente.Content = g.GetNome();
        NomeCpu.Content = cpu.GetNome();
        o = new Opzioni();
        o.nomeMazzo = m.GetNome();
        o.NomeCpu = cpu.GetNome();
        o.NomeUtente = g.GetNome();
        o.briscolaDaPunti = briscolaDaPunti;
        o.avvisaTalloneFinito = avvisaTalloneFinito;
        o.livello = (UInt16)(cbLivello.SelectedIndex + 1);
        o.stessoSeme = stessoSeme;
        GOpzioni.IsVisible = false;
        Applicazione.IsVisible = true;
        SalvaOpzioni(o);
        if (o.livello != helper.GetLivello() || stessoSeme != vecchioStessoSeme)
        {
            NuovaPartita(vecchioStessoSeme);
        }

    }
}
