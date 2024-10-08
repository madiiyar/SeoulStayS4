using SeoulStayS4.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SeoulStayS4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadAreas();
            LoadAmenities();
            LoadPropertyTypes();
        }

        private void LoadPropertyTypes()
        {
            using (var context = new SeoulStayS4Entities())
            {
                var property = context.ItemTypes.Select(n => new
                {
                    n.ID,
                    n.Name
                }).ToList();

                propertyTypeCombo.ItemsSource = property;
                propertyTypeCombo.DisplayMemberPath = "Name";
                propertyTypeCombo.SelectedValuePath = "ID";
            }
        }

        private void LoadAreas()
        {
            using (var context = new SeoulStayS4Entities())
            {
                areaCombo.ItemsSource = context.Areas.Select(a => a.Name).ToList();
            }
        }

        private void LoadAmenities()
        {
            using (var context = new SeoulStayS4Entities())
            {
                var amenity = context.Amenities.Select(a => a.Name).ToList();
                amenityCombo.ItemsSource = amenity;
                amenityCombo2.ItemsSource = amenity;
                amenityCombo3.ItemsSource = amenity;
            }
        }

        private void areaCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedArea = areaCombo.SelectedItem as string;

            if (!string.IsNullOrEmpty(selectedArea))
            {
                using (var context = new SeoulStayS4Entities())
                {
                    var areaId = context.Areas.FirstOrDefault(a => a.Name == selectedArea).ID;
                    attractionCombo.ItemsSource = context.Attractions.
                        Where(at => at.AreaID == areaId).
                        Select(i => i.Name).ToList();

                    titleCombo.ItemsSource = context.Items.
                        Where(i => i.AreaID == areaId).
                        Select(i => i.Title).ToList();
                }
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            simpleSearchGrid.Visibility = Visibility.Hidden;
            advancedSearchGrid.Visibility = Visibility.Visible;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            simpleSearchGrid.Visibility = Visibility.Visible;
            advancedSearchGrid.Visibility = Visibility.Hidden;
        }

        private void searchBtn_Click(object sender, RoutedEventArgs e)
        {
            string searching = searchProperties.Text;
            DateTime? dateTime = fromDate.SelectedDate;
            int? nights = string.IsNullOrEmpty(nightNum.Text) ? (int?)null : int.Parse(nightNum.Text);
            int? people = string.IsNullOrEmpty(peopleNum.Text) ? (int?)null : int.Parse(peopleNum.Text);
            DateTime current = DateTime.Now;

            if ( dateTime.Value.Date < current.Date)
            {
                MessageBox.Show("Date can not be earlier than current time", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            using (var context = new SeoulStayS4Entities())
            {
                var result = context.Items.AsQueryable();

                if (string.IsNullOrEmpty(searching))
                {
                    if (nights.HasValue)
                    {
                        result = result.Where(r => r.MinimumNights <= nights.Value);
                    }
                    if (people.HasValue)
                    {
                        result = result.Where(i => i.Capacity >= people.Value);
                    }
                }

                else
                {
                    result = result.Where(i => i.Title.Contains(searching) || i.Areas.Name.Contains(searching));
                    if (nights.HasValue)
                    {
                        result = result.Where(r => r.MaximumNights >= nights.Value);
                    }
                    if (people.HasValue)
                    {
                        result = result.Where(i => i.Capacity >= people.Value);
                    }
                }
                var results = result.Select(i => new
                    {
                    Property = i.Title,
                    Area = i.Areas.Name,
                    AverageScore = i.ItemScores.Any() ? i.ItemScores.Average(s => s.Value) : (double?)null, // Handle nullable AverageScores
                    TotalReservations = i.Users.Bookings.Any() ? i.Users.Bookings.Count() : 0, // If no reservations, return 0
                    AmountPayable = i.ItemPrices.FirstOrDefault() != null ? i.ItemPrices.FirstOrDefault().Price : (decimal?)null // Handle nullable Price
                }).ToList();

                simpleSearchDataGrid.ItemsSource = results;
            }
        }

        private void clearBtn_Click(object sender, RoutedEventArgs e)
        {
            areaCombo.SelectedIndex = -1;
            nightsNum2.Text = string.Empty;
            attractionCombo.SelectedIndex = -1;
            titleCombo.SelectedIndex = -1;
            amenityCombo.SelectedIndex = -1;
            fromDate2.SelectedDate = null;
            toDate.SelectedDate = null;
            peopleNum2.Text = string.Empty;
            strartPriceNum.Text = string.Empty;
            maxPriceNum.Text = string.Empty;
            amenityCombo.SelectedIndex = -1;
            amenityCombo2.SelectedIndex = -1;
            amenityCombo3.SelectedIndex = -1;
        }

        private void searchPropertyBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        
    }
}
