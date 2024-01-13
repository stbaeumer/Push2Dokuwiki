using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Push2Dokuwiki
{
    public class Schuelers : List<Schueler>
    {
        public Schuelers()
        {
        }

        public Schuelers(Klasses klasses)
        {
            Schuelers atlantisschulers = new Schuelers();

            using (OdbcConnection connection = new OdbcConnection(Global.ConnectionStringAtlantis))
            {
                DataSet dataSet = new DataSet();

                OdbcDataAdapter schuelerAdapter = new OdbcDataAdapter(@"SELECT   
            schueler.pu_id AS AtlantisSchuelerId,
            schueler.gebname AS Geburtsname,
            schueler.gebort_lkrs AS Geburtsort,
            schueler.s_geburts_land AS Geburtsland,
            schueler.s_joker_schueler_str_1 AS Wahlklausur12_1,
            schueler.s_joker_schueler_str_2 AS Wahlklausur12_2,
            schueler.s_joker_schueler_str_3 AS Wahlklausur13_1,
            schueler.s_joker_schueler_str_4 AS Wahlklausur13_2,
            schue_sj.vorgang_akt_satz_jn AS AktuellJN,
schue_sj.s_schulabschluss_bos AS Versetzung1,
schue_sj.dat_versetzung AS VersetzungsDatumInDiesenBildungsgang,
schue_sj.s_austritts_grund_bdlg AS BBB,
schue_sc.dat_austritt AS Austrittsdatum,
schue_sc.s_ausb_ziel_erreicht AS BeruflicherAbschluss,
schue_sc.s_austritt_schulabschluss AS AllgemeinerAbschluss,
schue_sc.s_hoechst_schulabschluss AS LSHoechAllAb,
schueler.s_schwerstbehindert AS XX,
schue_sj.kl_id AS AtlantisKlasseId,			
schue_sj.fko_id AS AtlantisStundentafelId,			
schue_sj.durchschnitts_note_jz AS DurchschnittsnoteJahreszeugnis,			
			schueler.name_1 AS Nachname,
			schueler.name_2 AS Vorname,
			check_null (hole_schuljahr_rech ('', -1))												as schuljahr_vorher, 
			check_null (hole_schuljahr_rech ('', 0))												as Bezugsjahr,				/*  1 aktuelles SJ des Users */
			check_null (sv_km_kuerzel_wert ('s_typ_vorgang', schue_sj.s_typ_vorgang))	as Status,  				/*  2 */
			/* lfd_nr im cf_ realisiert */  																							/*  3 */ 
			(if check_null (klasse.klasse_statistik_name) <> ''	THEN
				 check_null (klasse.klasse_statistik_name)         ELSE
				 check_null (klasse.klasse)								ENDIF)				as Klasse,  					/*  4 */
         check_null (sv_km_kuerzel_wert ('s_berufs_nr_gliederung', 
                                 schue_sj.s_berufs_nr_gliederung))   				as Gliederung,  				/*  5 */   
         check_null (substr(schue_sj.s_berufs_nr,4,5))				  					as Fachklasse,  				/*  6 Stelle 4-8 */  
			''																								as Klassenart,					/*  7 nicht BK */
			check_null (sv_km_kuerzel_wert ('s_klasse_art', klasse.s_klasse_art))	as OrgForm,  					/*  8 */
         check_null (sv_km_kuerzel_wert ('jahrgang', schue_sj.s_jahrgang))			as AktJahrgang,  				/*  9 */   
			check_null (sv_km_kuerzel_wert ('s_art_foerderungsbedarf', schueler.s_art_foerderungsbedarf))
																											as Foerderschwerp,			/* 10 */ 
			(if check_null (schueler.s_schwerstbehindert) = ''	THEN
				 '0'                 									ELSE
				 '1'															ENDIF)					as Schwerstbeh,				/* 11 */ 
			''																								as Reformpdg,					/* 12 */ 
			(if sv_steuerung ('s_unter', schueler.s_unter) = '$JVA' THEN
				 '1'                 									ELSE
				 '0'															ENDIF)		 				as JVA,							/* 13 */ 
         check_null_10(adresse.plz )                                             	as Plz, 				 			/* 14 */ 
			hole_schueler_ort_bundesland (adresse.s_gem_kz, adresse.ort, adresse.lkz)	as Ort,							/* 15 */
         schueler.dat_geburt			                    										as Gebdat,   					/* 16 */
         check_null (sv_km_kuerzel_wert ('s_geschl' , schueler.s_geschl))				as Geschlecht,   				/* 17 */   
         check_null (sv_km_kuerzel_wert ('s_staat'  , schueler.s_staat))				as Staatsang,   				/* 18 */
         
		   check_null (sv_km_kuerzel_wert ('s_bekennt', schueler.s_bekennt))       as Religion,  					/* 19 */
			schue_sj.dat_rel_anmeld																	as Relianmeldung,				/* 20 */
			schue_sj.dat_rel_abmeld																	as Reliabmeldung,				/* 21 */
			(if Aufnahmedatum_Bildungsgang is null THEN     		
				 Aufnahmedatum_Schule					ELSE
				 Aufnahmedatum_Bildungsgang			ENDIF)        							as	Aufnahmedatum,   			/* 22 */  
			(select max (lehr_sc.ls_kuerzel)
            from lehr_sc, kl_ls, lehrer
        	  where lehr_sc.ls_id 		= kl_ls.ls_id                                                   
        		 and kl_ls.kl_id 			= klasse.kl_id
				 and kl_ls.s_typ_kl_ls 	= '0'
             and lehrer.le_id 		= lehr_sc.le_id) 										as Labk,							/* 23 */
						
			(select adresse.plz
			   from adresse, betrieb, pj_bt  
			  where adresse.ad_id 		= betrieb.id_hauptadresse  
				 and pj_bt.bt_id 			= betrieb.bt_id    
				 and pj_bt.s_typ_pj_bt 	= '0'       
				 and pj_bt.pj_id 			= schue_sj.pj_id)										as ausbildort,  				/* 24 */                       
				
			hole_schueler_betriebsort (schue_sj.pj_id, 'ort')								as betriebsort,  				/* 25 */                       
				
			/* Kapitel der zuletzt besuchten Schule */
         check_null (sv_km_kuerzel_wert ('s_herkunfts_schule', 
                                 schue_sj.s_herkunfts_schule))                   as	LSSchulform,/* 26 */  
			left (schue_sj.vo_s_letzte_schule, 6) 						      		  		as LSSchulnummer,				/* 27 */
			check_null (sv_km_kuerzel_wert ('s_berufl_vorbildung_glied', 
                                 schue_sj.s_berufl_vorbildung_glied))        		as	LSGliederung,   			/* 28 */  
			substr (schue_sj.s_berufl_vorbildung, 4, 5) 			   						as	LSFachklasse,   			/* 29 */  
			''																								as LSKlassenart,				/* 30 */
			''																								as LSReformpdg,				/* 31 */
			Null																							as LSSchulentl,				/* 32 */		
			check_null (sv_km_kuerzel_wert ('s_abgang_jg', schue_sj.vo_s_jahrgang))	
																        									as	LSJahrgang,   				/* 33 */  
			(if Gliederung || Fachklasse = VOGliederung || VOFachklasse then 
             (if AktJahrgang = vojahrgang and Schueler_le_schuljahr_da = 'J' then 'W' else 'V' endif)   else
		      right (check_null (sv_km_kuerzel_wert ('s_schulabschluss', schue_sc.s_hoechst_schulabschluss)), 1) ENDIF)
																             							as LSQual,    					/* 34 */
			'0'																							as LSVersetz,					/* 35 */ 
	 
			/* Kapitel für das abgelaufene Schuljahr */
			check_null ((if (Fall_Bezugsjahr = '1')				THEN
				(SELECT klasse.klasse									/* aus Vorjahr lesen */  
					FROM klasse, schue_sj  
				  WHERE schue_sj.kl_id = klasse.kl_id     
					 and schue_sj.pj_id = VOpj_id)					ELSE
				/* Fall_schuljahr_vorher */
				klasse.klasse												ENDIF))				 	as VOKlasse,    				/* 36 */
			check_null ((if (Fall_Bezugsjahr = '1') 				THEN
				(select schue_sj.s_berufs_nr_gliederung			/* aus Vorjahr lesen */
					from schue_sj
				  where schue_sj.pj_id 	= VOpj_id)					ELSE
				/* Fall_schuljahr_vorher */
				schue_sj.s_berufs_nr_gliederung						ENDIF)) 					as VOGliederung,  			/* 37 */
			check_null ((if (Fall_Bezugsjahr = '1') 				THEN
				(select substr (schue_sj.s_berufs_nr, 4, 5)		/* aus Vorjahr lesen */ 
					from schue_sj
				  where schue_sj.pj_id 	= VOpj_id)					ELSE
				/* Fall_schuljahr_vorher */
				substr (schue_sj.s_berufs_nr, 4, 5)					ENDIF)) 					as VOFachklasse, 				/* 38 */
			check_null ((if (Fall_Bezugsjahr = '1')				THEN
				(SELECT sv_km_kuerzel_wert ('s_klasse_art', klasse.s_klasse_art)/* aus Vorjahr lesen */  
					FROM klasse, schue_sj  
				  WHERE schue_sj.kl_id = klasse.kl_id     
					 and schue_sj.pj_id = VOpj_id)					ELSE
				/* Fall_schuljahr_vorher */
				sv_km_kuerzel_wert ('s_klasse_art', klasse.s_klasse_art) ENDIF))		as VOOrgForm,    				/* 39 */
			''																								as VOKlassenart,				/* 40 nicht BK*/
			check_null ((if (Fall_Bezugsjahr = '1')				THEN
				(SELECT sv_km_kuerzel_wert ('jahrgang', schue_sj.s_jahrgang)/* aus Vorjahr lesen */  
					FROM klasse, schue_sj  
				  WHERE schue_sj.kl_id = klasse.kl_id     
					 and schue_sj.pj_id = VOpj_id)					ELSE
				/* Fall_schuljahr_vorher */
				sv_km_kuerzel_wert ('jahrgang', schue_sj.s_jahrgang) ENDIF))		   as VOJahrgang,    			/* 41 */
			check_null (sv_km_kuerzel_wert ('s_art_foerderungsbedarf_vj', 
                                 schueler.s_art_foerderungsbedarf_vj))   			as VOFoerderschwerp,			/* 42 */ 
			'0'																							as VOSchwerstbeh,				/* 43 */ 
			''																								as VOReformpdg,				/* 44 nicht BK*/
				
			hole_schueler_bildungsgang (schueler.pu_id, klasse.s_bildungsgang, schue_sj.dat_austritt, 'austritt')
																											as EntlDatum,					/* 45 */				
			check_null (sv_km_kuerzel_wert ('s_austritts_grund_bdlg', 
                                 schue_sj.s_austritts_grund_bdlg))               as Zeugnis_JZ, 				/* 46 */
			''																								as Schulpflichterf,			/* 47 nicht BK */ 			
			''																								as Schulwechselform,			/* 48 nicht BK */ 		
			''																								as Versetzung,					/* 49 */ 		
			(IF s_geburts_land is not null AND s_geburts_land <> '000' THEN
             string (year (dat_zuzug))
          ELSE
             ''
          ENDIF
			)																								as JahrZuzug_nicht_in_D_geboren,	/* 50 */
			string (year (dat_eintritt_gs))														as JahrEinschulung,					/* 51 */
			''																								as JahrWechselSekI,					/* 52 */
			(IF (s_geburts_land is not null AND s_geburts_land <> '000') OR (s_geburts_land is null AND dat_zuzug is not null) THEN
             '1'
          ELSE
             '0'
          ENDIF
			)																							   as Zugezogen,							/* 53 */
			/* Hilfsfelder */
			(SELECT max (adr_mutter.ad_id) 
            FROM adresse adr_mutter 
           WHERE adr_mutter.pu_id		= schueler.pu_id
             AND adr_mutter.s_typ_adr	= 'M'
			)																								as ad_id_mutter,
(select max (adresse.strasse)
			   from adresse  
			  where adresse.pu_id = schueler.pu_id)										as strasse, 
(select max (adresse.tel_1)
			   from adresse  
			  where adresse.pu_id = schueler.pu_id)										as telefon, 
			(SELECT adr_mutter.s_herkunftsland_adr 
            FROM adresse adr_mutter 
           WHERE adr_mutter.ad_id		= ad_id_mutter
			)																								as herkunftsland_mutter,			/* 54 */
			(SELECT max (adr_vater.ad_id) 
            FROM adresse adr_vater
           WHERE adr_vater.pu_id			= schueler.pu_id
             AND adr_vater.s_typ_adr	= 'V'
			)																								as ad_id_vater,
			(SELECT adr_vater.s_herkunftsland_adr 
            FROM adresse adr_vater
           WHERE adr_vater.ad_id			= ad_id_vater
			)																								as herkunftsland_vater,				/* 55 */
			(IF herkunftsland_mutter is not null AND herkunftsland_mutter <> '000'
             OR
				 herkunftsland_vater  is not null AND herkunftsland_vater  <> '000' THEN
             '1'
          ELSE
             '0'
          ENDIF
         )																								as Elternteilzugezogen,				/* 56 */
			(IF schueler.s_muttersprache is not null AND 
             schueler.s_muttersprache <> 'DE'     AND 
             schueler.s_muttersprache <> '000' THEN
             schueler.s_muttersprache
          ELSE
             ''
          ENDIF
         )																								as Verkehrssprache,					/* 57 */
			''																								as Einschulungsart					/* 58 */,
			''																								as Grundschulempfehlung				/* 59 */,
			schue_sj.pj_id																				as pj_id,
			check_null (sv_km_kuerzel_wert ('s_religions_unterricht', 
                                 schue_sj.s_religions_unterricht))					as s_religions_unterricht,
     		schue_sc.dat_eintritt	                   										as	Aufnahmedatum_schule,   			
			hole_schueler_bildungsgang (schueler.pu_id, klasse.s_bildungsgang, schue_sj.dat_eintritt, 'eintritt')
							     			                   										as	Aufnahmedatum_Bildungsgang,
			(SELECT max (pj_id) 
			   FROM schue_sj  
			  WHERE schue_sj.pu_id 					= schueler.pu_id 		and
					  schue_sj.s_typ_vorgang 		IN ('A', 'G', 'S')	and
					  schue_sj.vorgang_schuljahr 	= schuljahr_vorher) 						as VOpj_id,
			test_austritt (schue_sj.dat_austritt)												as ausgetreten,
		
			check_null ((SELECT noten_kopf.s_bestehen_absprf || noten_kopf.s_zusatzabschluss
			   FROM noten_kopf, schue_sj  
			  WHERE noten_kopf.s_typ_nok 			= 'HZ'
				 AND schue_sj.pj_id 					= noten_kopf.pj_id
				 AND schue_sj.pj_id 					= VOpj_id))            					as Zeugnis_HZ,
			check_null ((if ( schue_sj.vorgang_schuljahr 	= check_null (hole_schuljahr_rech ('',  0)) /* '2005/06' */
	  				AND schue_sj.vorgang_akt_satz_jn = 'J'
	  				AND ausgetreten 						= 'N') 		THEN
				 '1'                 									ELSE
				 '0'															ENDIF))					as Fall_Bezugsjahr,			
			check_null ((if ( schue_sj.vorgang_schuljahr 	= check_null (hole_schuljahr_rech ('', -1)) /* '2004/05' */
				   AND schue_sj.s_typ_vorgang 	IN ('A', 'G', 'S')
				   AND ausgetreten 					= 'J') 			THEN
				 '1'                 									ELSE
				 '0'															ENDIF))					as Fall_schuljahr_vorher,
			
			check_null ((if ( VOJahrgang <> '')							 			THEN
				 'J'                 									ELSE
				 'N'															ENDIF))					as Schueler_le_schuljahr_da,	
			
			check_null ((if ( Fachklasse <> VOFachklasse
					OR Gliederung <> VOGliederung)		 			THEN
				 'J'                 									ELSE
				 'N'															ENDIF))					as Schueler_Berufswechsel,	
			
			check_null ((if ( VoGliederung = 'C05' 
				  AND Gliederung = 'C06')		 						THEN
				 'J'                 									ELSE
				 'N'															ENDIF))					as Vorjahr_C05_Aktjahr_C06,		
			
			check_null (sv_km_kuerzel_wert ('jahrgang', schue_sj.s_jahrgang))				as schueler_jahrgang, 
			check_null ((if (Fall_Bezugsjahr = '1') 				THEN
				(select schue_sj.s_jahrgang			/* aus Vorjahr lesen */
					from schue_sj
				  where schue_sj.pj_id 	= VOpj_id)					ELSE
				/* Fall_schuljahr_vorher */
				schue_sj.s_jahrgang										ENDIF)) 					as VOSchueler_Jahrgang,
  			(IF EXISTS (SELECT 1 FROM schue_sj_info  
                      WHERE schue_sj_info.info_gruppe = 'MASSNA'
						      AND schue_sj_info.pj_id = schue_sj.pj_id) THEN '1' ELSE '0' ENDIF) as Massnahmetraeger,	/* 60 */
         check_null (sv_km_kuerzel_wert ('s_geschl', schueler.s_betreuungsart))	as Betreuung,   						/* 61 */   
  			(IF EXISTS (SELECT 1 FROM schueler_info  
                      WHERE schueler_info.info_gruppe = 'PUBEM'
                        AND schueler_info.s_typ_puin  = 'BKAZVO'
								AND schueler_info.betrag IN (0, 6, 12, 18)
						      AND schueler_info.pu_id = schueler.pu_id) THEN 
		   (SELECT konv_dec_string(schueler_info.betrag, 0)
			   FROM schueler_info  
			  WHERE schueler_info.puin_id = (SELECT max(puin.puin_id)  
														  FROM schueler_info puin 
														 WHERE puin.info_gruppe = 'PUBEM' 
														   AND puin.s_typ_puin = 'BKAZVO'
														   AND puin.betrag IN (0, 6, 12, 18)
														   AND puin.pu_id  = schueler.pu_id)) ELSE '0' ENDIF)	as BKAZVO,	/* 62 */   
			check_null (sv_km_kuerzel_wert ('s_art_foerderungsbedarf2', 
                                 schueler.s_art_foerderungsbedarf2))  	as Foerderschwerp2,						/* 63 */   
			check_null (sv_km_kuerzel_wert ('s_art_foerderungsbedarf_vj2',
                                 schueler.s_art_foerderungsbedarf_vj2)) as VOFoerderschwerp2,					/* 64 */   
			(IF schue_sc.berufsabschluss_jn = 'J' THEN 'Y' ELSE '' ENDIF)	as Berufsabschluss, 						/* 65 */   
			'Atlantis' 																		as Produktname, 						   /* 66 */   
			(SELECT 'V' || version.version_nr  
			   FROM version
           WHERE version.datum  = (SELECT max(v.datum) FROM version v)) as Produktversion,     			      /* 67 */   
         check_null (sv_km_kuerzel_wert ('s_bereich', klasse.s_bereich)) as Adressmerkmal,                  /* 68 */
			(if sv_steuerung ('s_unter', schueler.s_unter) = '$IN' THEN
				 '1'                 									ELSE
				 ''															ENDIF)		 as Internat,								/* 69 */ 
         '0'																			    as Koopklasse                      /* 70 */
  FROM   schueler,   
         schue_sc,  
         schue_sj,   
			schule,
         klasse,   
         adresse
 WHERE   schue_sj.kl_id 					= klasse.kl_id    			     
     AND schue_sc.sc_id 					= schule.sc_id  			    
     AND schue_sc.ps_id 					= schue_sj.ps_id  			     
     AND schue_sc.pu_id 					= schueler.pu_id  			     
	  AND adresse.ad_id 						= schueler.id_hauptadresse  
     AND (
			 (schue_sj.vorgang_schuljahr 	= check_null (hole_schuljahr_rech ('',  0)) /* aktuelles Jahr */
	  
			 )
          OR
          (
			  schue_sj.vorgang_schuljahr 	= check_null (hole_schuljahr_rech ('', -1)) /* letzte jahr */
	  AND   schue_sj.s_typ_vorgang 			IN ('G', 'S')
	  AND   ausgetreten = 'J'
			 )
         ) 
ORDER BY ausgetreten DESC, klasse, schueler.name_1, schueler.name_2", connection);



                connection.Open();
                schuelerAdapter.Fill(dataSet, "DBA.schueler");

                foreach (DataRow theRow in dataSet.Tables["DBA.schueler"].Rows)
                {
                    string vorname = theRow["Vorname"] == null ? "" : theRow["Vorname"].ToString();
                    string nachname = theRow["Nachname"] == null ? "" : theRow["Nachname"].ToString();

                    if (vorname.Length > 1 && nachname.Length > 1)
                    {
                        var schueler = new Schueler();
                        schueler.Id = theRow["AtlantisSchuelerId"] == null ? -99 : Convert.ToInt32(theRow["AtlantisSchuelerId"]);
                        schueler.Nachname = theRow["Nachname"] == null ? "" : theRow["Nachname"].ToString();
                        schueler.Vorname = theRow["Vorname"] == null ? "" : theRow["Vorname"].ToString();
                        schueler.Ort = theRow["Ort"] == null ? "" : theRow["Ort"].ToString();
                        schueler.Klasse = theRow["Klasse"] == null ? "" : theRow["Klasse"].ToString();
                        schueler.Wahlklausur12_1 = theRow["Wahlklausur12_1"] == null ? "" : theRow["Wahlklausur12_1"].ToString();
                        schueler.Wahlklausur12_2 = theRow["Wahlklausur12_2"] == null ? "" : theRow["Wahlklausur12_2"].ToString();
                        schueler.Wahlklausur13_1 = theRow["Wahlklausur13_1"] == null ? "" : theRow["Wahlklausur13_1"].ToString();
                        schueler.Wahlklausur13_2 = theRow["Wahlklausur13_2"] == null ? "" : theRow["Wahlklausur13_2"].ToString();
                        schueler.Gebdat = theRow["Gebdat"].ToString().Length < 3 ? new DateTime() : DateTime.ParseExact(theRow["Gebdat"].ToString(), "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        schueler.Telefon = theRow["telefon"] == null ? "" : theRow["telefon"].ToString();
                        schueler.Mail = schueler.Kurzname + "@students.berufskolleg-borken.de";
                        schueler.Eintrittsdatum = theRow["Aufnahmedatum"].ToString().Length < 3 ? new DateTime() : DateTime.ParseExact(theRow["Aufnahmedatum"].ToString(), "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                        schueler.AktuellJN = theRow["AktuellJN"] == null ? "" : theRow["AktuellJN"].ToString();

                        schueler.Austrittsdatum = theRow["Austrittsdatum"].ToString().Length < 3 ? new DateTime((DateTime.Now.Month >= 8 ? DateTime.Now.Year + 1 : DateTime.Now.Year), 7, 31) : DateTime.ParseExact(theRow["Austrittsdatum"].ToString(), "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        schueler.Volljährig = schueler.Gebdat.AddYears(18) > DateTime.Now ? false : true;
                        schueler.Geschlecht = theRow["Geschlecht"] == null ? "" : theRow["Geschlecht"].ToString();
                        schueler.Gebdat = theRow["Gebdat"].ToString().Length < 3 ? new DateTime() : DateTime.ParseExact(theRow["Gebdat"].ToString(), "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        schueler.Relianmeldung = theRow["Relianmeldung"].ToString().Length < 3 ? new DateTime() : DateTime.ParseExact(theRow["Relianmeldung"].ToString(), "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        schueler.Reliabmeldung = theRow["Reliabmeldung"].ToString().Length < 3 ? new DateTime() : DateTime.ParseExact(theRow["Reliabmeldung"].ToString(), "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                        schueler.Status = theRow["Status"] == null ? "" : theRow["Status"].ToString();
                        schueler.Bezugsjahr = Convert.ToInt32(theRow["Bezugsjahr"].ToString().Substring(0, 4)) - Convert.ToInt32(theRow["Fall_schuljahr_vorher"]);

                        schueler.LSSchulnummer = theRow["LSSchulnummer"] == null ? "" : theRow["LSSchulnummer"].ToString();

                        if (schueler.Bezugsjahr == (DateTime.Now.Month >= 8 ? DateTime.Now.Year : DateTime.Now.Year - 1) && schueler.Status != "VB" && schueler.Status != "8" && schueler.Status != "9" && schueler.Klasse != "Z" && schueler.AktuellJN == "J")
                        {
                            // Duplikate werden verhindert.

                            if (!(from s in this where s.Id == schueler.Id select s).Any())
                            {
                                atlantisschulers.Add(schueler);
                            }
                        }
                    }
                }

                var zz = (from s in atlantisschulers where s.Ort == "Heiden" select s).ToList();

                var scc = zz.Count();

                var cc = (from k in atlantisschulers where k.Status == "A" where k.Austrittsdatum > new DateTime(2020, 08, 10) where k.Austrittsdatum < DateTime.Now select k).ToList();

                using (SqlConnection sqlConnection = new SqlConnection(Global.ConnectionStringUntis))
                {
                    string sch = "";
                    try
                    {
                        string queryString = @"SELECT
StudNumber,
Email,
BirthDate,
FirstName,
Longname,
Name,
Flags,
Student_ID,
CLASS_ID
FROM Student 
WHERE SCHOOLYEAR_ID =" + Global.AktSj[0] + Global.AktSj[1] + ";";

                        SqlCommand sqlCommand = new SqlCommand(queryString, sqlConnection);
                        sqlConnection.Open();
                        SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();

                        List<Schueler> schuelers = new List<Schueler>();

                        while (sqlDataReader.Read())
                        {
                            Schueler schueler = new Schueler();
                            schueler.IdAtlantis = Convert.ToInt32(Global.SafeGetString(sqlDataReader, 0));
                            schueler.Mail = Global.SafeGetString(sqlDataReader, 1);

                            try
                            {
                                schueler.Gebdat = DateTime.ParseExact((sqlDataReader.GetInt32(2)).ToString(), "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture);
                            }
                            catch (Exception)
                            {
                            }
                            schueler.Vorname = Global.SafeGetString(sqlDataReader, 3);
                            schueler.Nachname = Global.SafeGetString(sqlDataReader, 4);

                            sch = schueler.Nachname + ", " + schueler.Vorname;

                            schueler.Anmeldename = Global.SafeGetString(sqlDataReader, 5);
                            schueler.GeschlechtMw = Global.SafeGetString(sqlDataReader, 6);
                            schueler.IdUntis = sqlDataReader.GetInt32(7);

                            var atlantisschueler = (from a in atlantisschulers
                                                    where a.Nachname == schueler.Nachname
                                                    where a.Vorname == schueler.Vorname
                                                    where a.Gebdat.Date == schueler.Gebdat.Date
                                                    select a).FirstOrDefault();


                            schueler.Klasse = (from a in atlantisschulers
                                               where a.Nachname == schueler.Nachname
                                               where a.Vorname == schueler.Vorname
                                               where a.Gebdat.Date == schueler.Gebdat.Date
                                               select a.Klasse).FirstOrDefault();

                            schueler.Reliabmeldung = (from a in atlantisschulers
                                                      where a.Nachname == schueler.Nachname
                                                      where a.Vorname == schueler.Vorname
                                                      where a.Gebdat.Date == schueler.Gebdat.Date
                                                      select a.Reliabmeldung).FirstOrDefault();

                            schueler.Relianmeldung = (from a in atlantisschulers
                                                      where a.Nachname == schueler.Nachname
                                                      where a.Vorname == schueler.Vorname
                                                      where a.Gebdat.Date == schueler.Gebdat.Date
                                                      select a.Relianmeldung).FirstOrDefault();

                            schuelers.Add(schueler);

                        };
                        sqlDataReader.Close();

                        schuelers = schuelers.OrderBy(o => o.Klasse).ToList();

                        foreach (var schueler in atlantisschulers)
                        {
                            this.Add(schueler);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                    finally
                    {
                        sqlConnection.Close();

                        Global.WriteLine("Schüler", this.Count);
                    }
                }
            }
        }

        internal void GetWebuntisUnterrichte(Unterrichts alleUnterrichte, Gruppen alleGruppen, string interessierendeKlasse, string hzJz, List<string> aktSj)
        {
            int i = 0;

            try
            {
                var unterrichteDerKlasse = (from a in alleUnterrichte
                                            where a.Klassen.Split('~').Contains(interessierendeKlasse)
                                            where a.Startdate <= DateTime.Now
                                            where a.Enddate >= DateTime.Now.AddMonths(-2) // Unterrichte, die 2 Monat vor Konferenz beendet wurden, zählen
                                            select a).ToList();


                if (hzJz == "JZ")
                {
                    // Im Jahreszeugnis der Unter- und Mittelstufen der Anlage A kann es sein, dass in einem
                    // Unterricht des 1.Hj Notes erteilt wurden, die aber noch nicht in F***lantis stehen.

                    var unterrichteDesErstenHj = (from a in alleUnterrichte
                                                  where a.Klassen.Split(',').Contains(interessierendeKlasse)
                                                  where a.Startdate >= new DateTime(Convert.ToInt32(aktSj[0]), 08, 01)
                                                  where a.Enddate <= new DateTime(Convert.ToInt32(aktSj[1]) + 2000, 02, 1)
                                                  where !(from u in unterrichteDerKlasse where u.Fach == a.Fach select u).Any()
                                                  select a).ToList();

                    unterrichteDerKlasse.AddRange(unterrichteDesErstenHj);
                }

                foreach (var schüler in this)
                {
                    i += schüler.GetUnterrichte(unterrichteDerKlasse, alleGruppen);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        internal void Notenlisten(
            string sqlPfad,
            string lokalerPfad,
            Unterrichts unterrichts,
            Lehrers lehrers,
            Klasses klasses)
        {
            var verschiedeneKlassen = (from s in
                                           this.OrderBy(x => x.Klasse)
                                       select s.Klasse).Distinct().ToList();

            File.WriteAllText(lokalerPfad + "start.txt", "====== Notenlisten Vollzeit ======" + Environment.NewLine);

            File.AppendAllText(lokalerPfad + "start.txt", Environment.NewLine);

            File.AppendAllText(lokalerPfad + "start.txt", "  Bitte diese Seite nicht manuell ändern." + Environment.NewLine);

            File.AppendAllText(lokalerPfad + "start.txt", Environment.NewLine);

            foreach (var klasse in verschiedeneKlassen)
            {
                File.AppendAllText(lokalerPfad + "start.txt", "  * [[:Notenlisten:" + klasse + " |" + klasse + "]]" + Environment.NewLine);

                Kurswahlen faecherUndNotenDerKlasse = new Kurswahlen();

                var verschiedeneSuSderKlasse = (from s in this where s.Klasse == klasse select s).ToList();

                var abfrage = "";

                foreach (var schuelerId in (from s in verschiedeneSuSderKlasse select s.Id))
                {
                    abfrage += "(DBA.schueler.pu_id = " + schuelerId + @") OR ";
                }

                try
                {
                    abfrage = abfrage.Substring(0, abfrage.Length - 4);
                }
                catch (Exception ex)
                {

                }
                try
                {
                    using (OdbcConnection connection = new OdbcConnection(Global.ConnectionStringAtlantis))
                    {
                        DataSet dataSet = new DataSet();

                        OdbcDataAdapter schuelerAdapter = new OdbcDataAdapter(@"
SELECT DBA.noten_einzel.noe_id AS LeistungId,
DBA.noten_einzel.fa_id,
DBA.noten_einzel.kurztext AS Fach,
DBA.noten_einzel.zeugnistext AS Zeugnistext,
DBA.noten_einzel.s_note AS Note,
DBA.noten_einzel.punkte AS Punkte,
DBA.noten_einzel.punkte_12_1 AS Punkte_12_1,
DBA.noten_einzel.punkte_12_2 AS Punkte_12_2,
DBA.noten_einzel.punkte_13_1 AS Punkte_13_1,
DBA.noten_einzel.punkte_13_2 AS Punkte_13_2,
DBA.noten_einzel.s_eingebracht_12_1 AS Eingebracht_12_1,
DBA.noten_einzel.s_eingebracht_12_2 AS Eingebracht_12_2,
DBA.noten_einzel.s_eingebracht_13_1 AS Eingebracht_13_1,
DBA.noten_einzel.s_eingebracht_13_2 AS Eingebracht_13_2,
DBA.noten_einzel.s_abiturfach AS Abiturfach,
DBA.noten_einzel.s_tendenz AS Tendenz,
DBA.noten_einzel.s_einheit AS Einheit,
DBA.noten_einzel.ls_id_1 AS LehrkraftAtlantisId,
DBA.noten_einzel.position_1 AS Reihenfolge,
DBA.schueler.name_1 AS Nachname,
DBA.schueler.name_2 AS Vorname,
DBA.schueler.dat_geburt,
DBA.schueler.pu_id AS SchlüsselExtern,
DBA.schue_sj.s_religions_unterricht AS Religion,
DBA.schue_sj.dat_austritt AS ausgetreten,
DBA.schue_sj.dat_rel_abmeld AS DatumReligionAbmeldung,
DBA.schue_sj.vorgang_akt_satz_jn AS SchuelerAktivInDieserKlasse,
DBA.schue_sj.vorgang_schuljahr AS Schuljahr,
(substr(schue_sj.s_berufs_nr,4,5)) AS Fachklasse,
DBA.klasse.s_klasse_art AS Anlage,
DBA.klasse.jahrgang AS Jahrgang,
DBA.schue_sj.s_gliederungsplan_kl AS Gliederung,
DBA.noten_kopf.s_typ_nok AS HzJz,
DBA.noten_kopf.unterzeichner_rechts AS Klassenleiter,
DBA.noten_kopf.unterzeichner_rechts_text AS Klassenleitername,
DBA.noten_kopf.nok_id AS NOK_ID,
s_art_fach,
DBA.noten_kopf.s_art_nok AS Zeugnisart,
DBA.noten_kopf.bemerkung_block_1 AS Bemerkung1,
DBA.noten_kopf.bemerkung_block_2 AS Bemerkung2,
DBA.noten_kopf.bemerkung_block_3 AS Bemerkung3,
DBA.noten_kopf.dat_notenkonferenz AS Konferenzdatum,
DBA.klasse.klasse AS Klasse
FROM(((DBA.noten_kopf JOIN DBA.schue_sj ON DBA.noten_kopf.pj_id = DBA.schue_sj.pj_id) JOIN DBA.klasse ON DBA.schue_sj.kl_id = DBA.klasse.kl_id) JOIN DBA.noten_einzel ON DBA.noten_kopf.nok_id = DBA.noten_einzel.nok_id ) JOIN DBA.schueler ON DBA.noten_einzel.pu_id = DBA.schueler.pu_id
WHERE schue_sj.s_typ_vorgang = 'A' AND (s_typ_nok = 'JZ' OR s_typ_nok = 'HZ' OR s_typ_nok = 'GO') AND
(  
  " + abfrage + @"
)
ORDER BY DBA.klasse.s_klasse_art DESC, DBA.noten_kopf.dat_notenkonferenz DESC, DBA.klasse.klasse ASC, DBA.noten_kopf.nok_id, DBA.noten_einzel.position_1; ", connection);

                        connection.Open();
                        schuelerAdapter.Fill(dataSet, "DBA.leistungsdaten");

                        string bereich = "";

                        foreach (DataRow theRow in dataSet.Tables["DBA.leistungsdaten"].Rows)
                        {
                            if (theRow["s_art_fach"].ToString() == "U")
                            {
                                bereich = theRow["Zeugnistext"].ToString();
                            }
                            else
                            {
                                DateTime austrittsdatum = theRow["ausgetreten"].ToString().Length < 3 ? new DateTime() : DateTime.ParseExact(theRow["ausgetreten"].ToString(), "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                                Kurswahl noten = new Kurswahl();

                                try
                                {
                                    // Wenn der Schüler nicht in diesem Schuljahr ausgetreten ist ...

                                    if (!(austrittsdatum > new DateTime(DateTime.Now.Month >= 8 ? DateTime.Now.Year : DateTime.Now.Year - 1, 8, 1) && austrittsdatum < DateTime.Now))
                                    {
                                        noten.LeistungId = Convert.ToInt32(theRow["LeistungId"]);
                                        noten.NokId = Convert.ToInt32(theRow["NOK_ID"]);
                                        noten.SchlüsselExtern = Convert.ToInt32(theRow["SchlüsselExtern"]);
                                        noten.Schuljahr = theRow["Schuljahr"].ToString();
                                        noten.Gliederung = theRow["Gliederung"].ToString();
                                        noten.Abifach = theRow["Abiturfach"].ToString();
                                        noten.HatBemerkung = (theRow["Bemerkung1"].ToString() + theRow["Bemerkung2"].ToString() + theRow["Bemerkung3"].ToString()).Contains("Fehlzeiten") ? true : false;
                                        noten.Jahrgang = Convert.ToInt32(theRow["Jahrgang"].ToString().Substring(3, 1));
                                        noten.Name = theRow["Nachname"] + " " + theRow["Vorname"];
                                        noten.Nachname = theRow["Nachname"].ToString();
                                        noten.Vorname = theRow["Vorname"].ToString();

                                        noten.KlassenleiterName = theRow["Klassenleitername"].ToString();
                                        noten.Klassenleiter = theRow["Klassenleiter"].ToString();

                                        if ((theRow["LehrkraftAtlantisId"]).ToString() != "")
                                        {
                                            noten.LehrkraftAtlantisId = Convert.ToInt32(theRow["LehrkraftAtlantisId"]);
                                        }
                                        noten.Bereich = bereich;
                                        try
                                        {
                                            noten.Reihenfolge = Convert.ToInt32(theRow["Reihenfolge"].ToString());
                                        }
                                        catch (Exception)
                                        {
                                            throw;
                                        }

                                        noten.Geburtsdatum = theRow["dat_geburt"].ToString().Length < 3 ? new DateTime() : DateTime.ParseExact(theRow["dat_geburt"].ToString(), "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                                        noten.Volljährig = noten.Geburtsdatum.AddYears(18) > DateTime.Now ? false : true;
                                        noten.Klasse = theRow["Klasse"].ToString();
                                        noten.Fach = theRow["Fach"] == null ? "" : theRow["Fach"].ToString();
                                        noten.Gesamtnote = theRow["Note"].ToString() == "" ? null : theRow["Note"].ToString() == "Attest" ? "A" : theRow["Note"].ToString();
                                        noten.Gesamtpunkte_12_1 = theRow["Punkte_12_1"].ToString() == "" ? null : (theRow["Punkte_12_1"].ToString()).Split(',')[0];
                                        noten.Gesamtpunkte_12_2 = theRow["Punkte_12_2"].ToString() == "" ? null : (theRow["Punkte_12_2"].ToString()).Split(',')[0];
                                        noten.Gesamtpunkte_13_1 = theRow["Punkte_13_1"].ToString() == "" ? null : (theRow["Punkte_13_1"].ToString()).Split(',')[0];
                                        noten.Gesamtpunkte_13_2 = theRow["Punkte_13_2"].ToString() == "" ? null : (theRow["Punkte_13_2"].ToString()).Split(',')[0];
                                        noten.Gesamtpunkte = theRow["Punkte"].ToString() == "" ? null : (theRow["Punkte"].ToString()).Split(',')[0];
                                        noten.Eingebracht_12_1 = theRow["Eingebracht_12_1"].ToString() == "" ? null : theRow["Eingebracht_12_1"].ToString();
                                        noten.Eingebracht_12_2 = theRow["Eingebracht_12_2"].ToString() == "" ? null : theRow["Eingebracht_12_2"].ToString();
                                        noten.Eingebracht_13_1 = theRow["Eingebracht_13_1"].ToString() == "" ? null : theRow["Eingebracht_13_1"].ToString();
                                        noten.Eingebracht_13_2 = theRow["Eingebracht_13_2"].ToString() == "" ? null : theRow["Eingebracht_13_2"].ToString();
                                        noten.Tendenz = theRow["Tendenz"].ToString() == "" ? null : theRow["Tendenz"].ToString();
                                        noten.EinheitNP = theRow["Einheit"].ToString() == "" ? "N" : theRow["Einheit"].ToString();
                                        noten.SchlüsselExtern = Convert.ToInt32(theRow["SchlüsselExtern"].ToString());
                                        noten.HzJz = theRow["HzJz"].ToString();
                                        noten.Anlage = theRow["Anlage"].ToString();
                                        noten.Zeugnisart = theRow["Zeugnisart"].ToString();
                                        noten.Zeugnistext = theRow["Zeugnistext"].ToString();
                                        noten.Konferenzdatum = theRow["Konferenzdatum"].ToString().Length < 3 ? new DateTime() : (DateTime.ParseExact(theRow["Konferenzdatum"].ToString(), "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture)).AddHours(15);
                                        noten.DatumReligionAbmeldung = theRow["DatumReligionAbmeldung"].ToString().Length < 3 ? new DateTime() : DateTime.ParseExact(theRow["DatumReligionAbmeldung"].ToString(), "dd.MM.yyyy HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                                        noten.SchuelerAktivInDieserKlasse = theRow["SchuelerAktivInDieserKlasse"].ToString() == "J";
                                        noten.Beschreibung = "";


                                        noten.Lehrkraft = (from u in unterrichts
                                                           where u.KlasseKürzel == noten.Klasse
                                                           where u.FachKürzel == noten.Fach.Replace("  ", " ")
                                                           select u.LehrerKürzel).FirstOrDefault();

                                        if (noten.Lehrkraft == null)
                                        {
                                            var fach = (from u in unterrichts
                                                        where u.KlasseKürzel == noten.Klasse
                                                        where u.FachKürzel == noten.Fach.Replace("  ", " ")
                                                        select u.FachKürzel).FirstOrDefault();
                                            noten.Lehrkraft = "";

                                            //           Console.WriteLine(noten.Klasse + ": Atlantis: " + noten.Fach + " <-> Untis: -");
                                        }

                                        if (klasse == noten.Klasse)
                                        {
                                            faecherUndNotenDerKlasse.Add(noten);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {

                                }
                            }
                        }
                        connection.Close();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                File.WriteAllText(lokalerPfad + klasse + ".txt", "====== Notenlisten " + klasse + " ======" + Environment.NewLine);

                File.AppendAllText(lokalerPfad + klasse + ".txt", Environment.NewLine);

                File.AppendAllText(lokalerPfad + klasse + ".txt", "  Bitte diese Seite nicht manuell ändern." + Environment.NewLine);

                File.AppendAllText(lokalerPfad + klasse + ".txt", Environment.NewLine);

                var verschiedeneKonferenzdatenInDiesemSJ = (from n in faecherUndNotenDerKlasse.OrderByDescending(x => x.Konferenzdatum)
                                                            where n.Klasse == klasse
                                                            where n.Konferenzdatum > new DateTime(Convert.ToInt32(Global.AktSj[0]), 10, 01)
                                                            where n.Konferenzdatum < new DateTime(Convert.ToInt32(Global.AktSj[1]), 08, 01)
                                                            select n.Konferenzdatum).Distinct().ToList();

                foreach (var konferenzdatum in verschiedeneKonferenzdatenInDiesemSJ)
                {
                    File.AppendAllText(lokalerPfad + klasse + ".txt", "==== " + klasse + " Konferenzdatum:" + konferenzdatum.ToShortDateString() + " ====" + Environment.NewLine);

                    File.AppendAllText(lokalerPfad + klasse + ".txt", Environment.NewLine);

                    var schuelerDieserKlasse = (from s in this
                                                where s.Klasse == klasse
                                                select s).ToList().OrderBy(x => x.Nachname).ThenBy(x => x.Vorname);

                    var alleFächer = (from t in faecherUndNotenDerKlasse.OrderBy(x => x.Reihenfolge)
                                      where t.Klasse == klasse
                                      where t.Konferenzdatum == konferenzdatum
                                      select new { t.Fach, t.Bereich, t.Lehrkraft }).Distinct().ToList();

                    var kopfzeile1 = "^                                 ^^" + alleFächer[1].Bereich;
                    var kopfzeile2 = "^:::                              ^^";
                    var kopfzeile3 = "^:::                              ^^";

                    for (int i = 0; i < alleFächer.Count; i++)
                    {
                        if (i > 0 && alleFächer[i - 1].Bereich != alleFächer[i].Bereich)
                        {
                            kopfzeile1 += alleFächer[i].Bereich + "  ^";
                        }
                        else
                        {
                            kopfzeile1 += "^";
                        }

                        kopfzeile2 += alleFächer[i].Fach.PadRight(5) + "^";
                        kopfzeile3 += "  " + alleFächer[i].Lehrkraft.PadRight(5) + "^";
                    }

                    File.AppendAllText(lokalerPfad + klasse + ".txt", kopfzeile1 + Environment.NewLine);
                    File.AppendAllText(lokalerPfad + klasse + ".txt", kopfzeile2 + Environment.NewLine);
                    File.AppendAllText(lokalerPfad + klasse + ".txt", kopfzeile3 + Environment.NewLine);
                    int y = 1;

                    foreach (var schueler in schuelerDieserKlasse)
                    {
                        var fächerDesSchülers = (from t in faecherUndNotenDerKlasse
                                                 where t.SchlüsselExtern == schueler.Id
                                                 where t.Konferenzdatum == konferenzdatum
                                                 select t).ToList();

                        var zeile = "|" + y.ToString().PadLeft(3) + ".|" + (from t in faecherUndNotenDerKlasse where t.SchlüsselExtern == schueler.Id select t.Nachname.Substring(0, 2) + ", " + t.Vorname.Substring(0, 2)).FirstOrDefault().PadRight(27) + "  |";

                        y++;

                        foreach (var fach in alleFächer)
                        {
                            var noteDesSchülers = (from f in fächerDesSchülers
                                                   where f.Fach == fach.Fach
                                                   where f.Konferenzdatum == konferenzdatum
                                                   select f.Gesamtnote).FirstOrDefault();

                            zeile += (noteDesSchülers != null ? "  " + noteDesSchülers + "  " : "     ") + "|";
                        }

                        File.AppendAllText(lokalerPfad + klasse + ".txt", zeile.TrimEnd(' ') + Environment.NewLine);
                    }

                    File.AppendAllText(lokalerPfad + klasse + ".txt", "" + Environment.NewLine);
                    File.AppendAllText(lokalerPfad + klasse + ".txt", "" + Environment.NewLine);
                }

                File.AppendAllText(lokalerPfad + klasse + ".txt", "" + Environment.NewLine);
                File.AppendAllText(lokalerPfad + klasse + ".txt", "Seite erstellt mit [[github>stbaeumer/Push2Dokuwiki|Push2Dokuwiki]]." + Environment.NewLine);

                File.AppendAllText(lokalerPfad + klasse + ".txt", "" + Environment.NewLine);
                File.AppendAllText(lokalerPfad + klasse + ".txt", "{{tag>Notenlisten}}" + Environment.NewLine);
                Global.DateiTauschen(sqlPfad + klasse + ".txt", lokalerPfad + klasse + ".txt");
                //break;
            }
            Global.DateiTauschen(sqlPfad + "start.txt", lokalerPfad + "start.txt");
        }

        internal void Reliabmelder(string dokuwikipfadUndDatei, string reliabwaehlerNeu)
        {
            var abgemeldeteSchuelers = (from s in this
                                        where s.Reliabmeldung.Year > 1
                                        where s.Reliabmeldung <= DateTime.Now.Date
                                        where !(s.Relianmeldung > s.Reliabmeldung) //nicht wieder angemeldet
                                        select s).OrderBy(x => x.Klasse).ThenBy(x => x.Nachname).ThenBy(x => x.Vorname).ToList();

            File.WriteAllText(reliabwaehlerNeu, "~~NOTOC~~" + Environment.NewLine);

            File.AppendAllText(reliabwaehlerNeu, "====== Abgemeldete vom Religionsunterricht ======" + Environment.NewLine);

            File.AppendAllText(reliabwaehlerNeu, "" + Environment.NewLine);
            File.AppendAllText(reliabwaehlerNeu, "  * [[religionslehre|Religionslehre]]" + Environment.NewLine);
            File.AppendAllText(reliabwaehlerNeu, "  * [[fachschaften|Fachschaft]]: [[fachschaften:religionslehre|Religion]]" + Environment.NewLine);
            File.AppendAllText(reliabwaehlerNeu, "" + Environment.NewLine);

            File.AppendAllText(reliabwaehlerNeu, "  Diese Seite wird automatisch aktualisiert. Bitte nicht manuell ändern." + Environment.NewLine);

            File.AppendAllText(reliabwaehlerNeu, Environment.NewLine);

            File.AppendAllText(reliabwaehlerNeu, "<searchtable>" + Environment.NewLine);

            File.AppendAllText(reliabwaehlerNeu, "^ ^Klasse^Name^Abgemeldet am^" + Environment.NewLine);

            for (int i = 0; i < abgemeldeteSchuelers.Count; i++)
            {
                File.AppendAllText(reliabwaehlerNeu, "|  " + (i + 1).ToString().PadRight(3) + ".|" + abgemeldeteSchuelers[i].Klasse.PadRight(8) + "|" + (abgemeldeteSchuelers[i].Nachname + ", " + abgemeldeteSchuelers[i].Vorname).PadRight(30) + "|" + abgemeldeteSchuelers[i].Reliabmeldung.ToShortDateString() + "|" + Environment.NewLine);
            }

            File.AppendAllText(reliabwaehlerNeu, "</searchtable>" + Environment.NewLine);
            File.AppendAllText(reliabwaehlerNeu, "" + Environment.NewLine);
            File.AppendAllText(reliabwaehlerNeu, "Seite erstellt mit [[github>stbaeumer/Push2Dokuwiki|Push2Dokuwiki]]." + Environment.NewLine);

            File.AppendAllText(reliabwaehlerNeu, "" + Environment.NewLine);
            File.AppendAllText(reliabwaehlerNeu, "{{tag>Religionslehre}}" + Environment.NewLine);

            Global.DateiTauschen(dokuwikipfadUndDatei, reliabwaehlerNeu);
        }
    }
}