# Art of Magic-Crawler
MagicCrawler is a C# windows forms application that scrapes 
images from the 
[Art of Magic-the-Gathering project](https://www.artofmtg.com/)
and deploys them via a windows form s.t. other software can make use
of the scraped images in a user-friendly way.

This software is thought as a tool for other C# desktop applications, with the main focus being on software for creating games or TT-RPG assets
(like the [Werwolf-Card Generator Application](https://github.com/Semigroup/Werwolf) for example).

## Scraping artofmtg.com
This software crawls the website artofmtg.com
and downloads all pictures found there together with relevant information on a specified path.
Additionally, it downloads [AtomicCards.json](https://mtgjson.com/api/v5/AtomicCards.json)
and uses this JSon database to equip the downloaded images with additional information.

## Main Dialog Elements of this Project
The center of this project is the the following class:
```c#
public class LibraryImageSelectionDialog : Form
{
    //a lot of fields are omitted here...

    private ShowArtSideForm SideForm;

    //a lot of methods are omitted here...

    public void SetLibrary(ArtLibrary library);
    public DialogResult ShowDialog();
}
```

A LibraryImageSelectionDialog is a Windows Form that allows to quickly scroll and browse through all pictures of artofmtg.com.
It has a text field where one can enter key words to filter all images of MtG-cards whose text, name, type or colors contain the given key.
In this form, a user can pin and select images by clicking on them, they will be pinned at the top with the current selection being bordered by a dashed red line.
After a user has found a satisfying image, they can confirm their choice by clicking the corresponding button of the form.

Additionally, when show this form additionally opens a ```ShowArtSideForm```.
Whenever the user hovers with his mouse over an image in the main form, the side form will display an enlarged version of
it together with the text from AtomicCards.json that is used for filtering by keys.

## Usage

To use this software download the source of this repo
and add a reference in your project to this project (or to a .dll of this project).
Additionally, you need to download the source of the [EBookCrawler project](https://github.com/Semigroup/EBookCrawler)
(or add its .dll to your project).

Before using the form, your software needs
to first crawl artofmtg.com and download all images and necessary files somewhere.
For this end, you software should call once
```c#
    Creator.UpdateMtgLibrary(root);
```
where ```root``` is a path to an existing folder where you want to save all images scraped from artofmtg.com together with additional data.
You can call this method again if you want to update your library.

Note, that creating and updating this library takes several hours.
So, your software shouldn't call this method regularly.

After that, the default way to use this tool is to call the following methods.
```c#
    var imageSelectionDialog = new LibraryImageSelectionDialog();
    var lib = ArtLibrary.ReadLibrary(root);
    imageSelectionDialog.SetLibrary(lib);

    if (imageSelectionDialog.ShowDialog() == DialogResult.OK)
    {
        string imagePath = imageSelectionDialog.SelectedTile.Art.AbsoluteImagePath;

        // if the user confirmed the choice of a picture, then imagePath is the absolute path to the chosen image
    }
```

