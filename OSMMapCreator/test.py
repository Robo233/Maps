import math
import xml.etree.ElementTree as ET
from PIL import Image, ImageDraw, ImageColor

osm_file  = "C:/Users/Robi/Desktop/OSMMapCreator/map.osm"

upper_right_latitude = 45.6503
upper_right_longitude = 25.6060
lower_left_latitude = 45.6288
lower_left_longitude = 25.5759


scale_factor = 1000000 # the bigger this is, the bigger the image will be

number_of_files = 256 # the number of images that will be generated. This becomes the closest square number to this number
border_width = 1
number_of_minimum_pixels = 5 # an image will only be generated if it has at least this amount of non-background_color pixels


background_color = "#434343"
border_color = "#5e5e5e"
building_color = "#ababab"


def extract_polygons_from_osm(osm_file):
    # Parse the XML
    tree = ET.parse(osm_file)
    root = tree.getroot()

    # Create a dictionary of nodes
    nodes = {}
    for node in root.findall('.//node'):
        id = node.get('id')
        lat = float(node.get('lat'))
        lon = float(node.get('lon'))
        nodes[id] = (lon, lat)

    # Find all buildings
    buildings = []
    for way in root.findall('.//way'):
        is_building = False
        for tag in way.findall('.//tag'):
            if tag.get('k') == 'building':
                is_building = True
                break
        if is_building:
            building_coords = []
            for nd in way.findall('.//nd'):
                node_id = nd.get('ref')
                building_coords.append(nodes[node_id])
            buildings.append(building_coords)

    return buildings

def geo_to_image_coords(geo_coords, center, scale_factor):
    lon, lat = geo_coords
    dx = lon - center[0]
    dy = lat - center[1]
    x = scale_factor * dx
    y = - scale_factor * dy
    return x, y

# Extract building polygons
buildings = extract_polygons_from_osm(osm_file)

# Determine the center of the map
center_lat = (lower_left_latitude + upper_right_latitude) / 2
center_lon = (lower_left_longitude + upper_right_longitude) / 2

center = (center_lon, center_lat)


img_width = round((upper_right_longitude - lower_left_longitude) * scale_factor)
img_height = round((upper_right_latitude - lower_left_latitude) * scale_factor)
img = Image.new('RGB', (img_width, img_height), background_color)

draw = ImageDraw.Draw(img)

# Calculate the bounding box of the scaled building coordinates
min_x = float('inf')
max_x = float('-inf')
min_y = float('inf')
max_y = float('-inf')
for building in buildings:
    for coord in building:
        x, y = geo_to_image_coords(coord, center, scale_factor)
        min_x = min(min_x, x)
        max_x = max(max_x, x)
        min_y = min(min_y, y)
        max_y = max(max_y, y)

# Calculate the offsets
offset_x = (img.size[0] - (max_x - min_x)) / 2 - min_x
offset_y = (img.size[1] - (max_y - min_y)) / 2 - min_y

for building in buildings:
    building_image_coords = [(
        int(x + offset_x),
        int(y + offset_y)
    ) for x, y in [geo_to_image_coords(coord, center, scale_factor) for coord in building]]
    
    # Check if building_image_coords has at least 2 coordinates
    if len(building_image_coords) < 2:
        continue
    
    # Fill the polygon with black
    draw.polygon(building_image_coords, fill=building_color)

    # Draw the polygon's border using lines for a thicker outline
    for i in range(len(building_image_coords)):
        start_point = building_image_coords[i]
        end_point = building_image_coords[(i + 1) % len(building_image_coords)]  # Wrap around to the start for the last point
        draw.line([start_point, end_point], fill=border_color, width=border_width)

   

img.show()
img.save("C:/Users/Robi/Desktop/OSMMapCreator/output_map.jpg", "JPEG")

def count_non_bg_pixels(img, bg_color):
    """Count the number of non-background-color pixels."""
    return sum(1 for pixel in img.getdata() if pixel != tuple(map(int, ImageColor.getrgb(bg_color))))



tiles_per_side = int(math.sqrt(number_of_files))

# Slice the large image into smaller tiles
tile_width = round(img_width / tiles_per_side)
tile_height = round(img_height / tiles_per_side)


tile_counter = 0  # Added to keep track of naming

for i in range(tiles_per_side):
    for j in range(tiles_per_side):
        left = i * tile_width
        upper = (tiles_per_side-1-j) * tile_height
        right = left + tile_width
        lower = upper + tile_height
        tile = img.crop((left, upper, right, lower))
        tile_counter += 1  # Increment the counter regardless of whether you save the image or not
        
        # Check if the tile meets the pixel count criteria
        if count_non_bg_pixels(tile, background_color) >= number_of_minimum_pixels:
            tile.save(f"C:/Users/Robi/Desktop/OSMMapCreator/output_map_tile_{tile_counter}.png", "PNG")
