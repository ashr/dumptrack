# dumptrack
.Net hack dump tracker

This little utility uses a custom google search from 'Paste Search by NetBootCamp (90+ sites)'. 

We use it internally to find leaked accounts from paste sites, but you could use it to search for and report on anything you want.

The file offencelist.txt contains all the terms that dumptracker will query google for. It will query each item in the file one by one and suck in all the results for the query, no matter how many pages of results. Once it has all the query result urls it will download each and check inside the file for the term you searched for. If it finds it, it will write it to a file in the format offencelist-" + DateTime.Now.ToString ("yyyy-MM-dd") + ".txt, the file will contain the term it found and the url of where it found it.

You can define and use your own custom search if required, it requires the 'cx' custom search identifier from Google Api's if you do. 

You will also have to create your own Google API Key to be able to use this utility, Google allows for 100 free searches every day.

For API Keys:
You can get an API key by visiting https://code.google.com/apis/console and clicking "API Access". You will then need to switch on the custom search API on the "Services" tab.

For your own Custom Search:
https://developers.google.com/custom-search/

---

Also searching github commits now for offences listed in another file added blacklist functionality to remove items that are not applicable and always come up in the search (like newstools repository)

The design has been modularised to make it easy to add modules that search other places/sites - i will build in onion search functionality soon-ish.

Please help me make this tool a great OSINT toy, our organisation already got massive benefit from it.


